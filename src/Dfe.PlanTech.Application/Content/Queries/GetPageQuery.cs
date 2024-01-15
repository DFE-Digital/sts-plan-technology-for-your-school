using AutoMapper;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageQuery : ContentRetriever, IGetPageQuery
{
    private readonly ICmsDbContext _db;
    private readonly ILogger<GetPageQuery> _logger;
    private readonly IQuestionnaireCacher _cacher;
    private readonly IMapper _mapperConfiguration;

    readonly string _getEntityEnvVariable = Environment.GetEnvironmentVariable("CONTENTFUL_GET_ENTITY_INT") ?? "4";

    public GetPageQuery(ICmsDbContext db, ILogger<GetPageQuery> logger, IMapper mapperConfiguration, IQuestionnaireCacher cacher, IContentRepository repository) : base(repository)
    {
        _cacher = cacher;
        _db = db;
        _logger = logger;
        _mapperConfiguration = mapperConfiguration;
    }

    /// <summary>
    /// Fetches page from <see chref="IContentRepository"/> by slug
    /// </summary>
    /// <param name="slug">Slug for the Page</param>
    /// <returns>Page matching slug</returns>
    public async Task<Page> GetPageBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var matchingPage = await GetPageFromDatabase(slug, cancellationToken);

        if (!IsValidPage(matchingPage, slug))
        {
            return await GetPageFromContentful(slug, cancellationToken);
        }

        await TryLoadButtonReferences(matchingPage!, cancellationToken);
        await TryLoadCategorySections(matchingPage!, cancellationToken);
        await TryLoadRichTextContents(matchingPage!, cancellationToken);

        var mapped = _mapperConfiguration.Map<PageDbEntity, Page>(matchingPage!);

        UpdateSectionTitle(mapped);

        return mapped;
    }

    /// <summary>
    /// Checks the retrieved Page from the DB to ensure it is valid (e.g. not null, has content). If not, logs message and returns false.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="slug"></param>
    /// <returns></returns>
    private bool IsValidPage(PageDbEntity? page, string slug)
    {
        if (page == null)
        {
            _logger.LogInformation("Could not find page {slug} in DB - checking Contentful", slug);
            return false;
        }

        if (page.Content == null || page.Content.Count == 0)
        {
            _logger.LogWarning("Page {slug} has no 'Content' in DB - checking Contentful", slug);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Retrieve the page for the slug from the database
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<PageDbEntity?> GetPageFromDatabase(string slug, CancellationToken cancellationToken)
    {
        try
        {
            var page = await _db.GetPageBySlug(slug, cancellationToken);

            if (page == null) _logger.LogInformation("Found no matching page for {slug} in database", slug);

            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching {page} from database", slug);
            throw;
        }
    }

    /// <summary>
    /// Load RichTextContents for page from database 
    /// </summary>
    /// <param name="page"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task TryLoadRichTextContents(PageDbEntity page, CancellationToken cancellationToken)
    {
        try
        {
            var textBodyContents = page.Content.Concat(page.BeforeTitleContent)
                                                .OfType<IHasRichText>()
                                                .ToArray();

            if (!textBodyContents.Any()) return;

            await _db.ToListAsync(_db.RichTextContentsByPageSlug(page.Slug), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching rich text content from Database for {pageId}", page.Id);
            throw;
        }
    }

    /// <summary>
    /// If tbere are any "Category" components in the Page.Content, then load the required Section information for each one.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task TryLoadCategorySections(PageDbEntity page, CancellationToken cancellationToken)
    {
        try
        {
            var pageHasCategories = page.Content.Exists(content => content is CategoryDbEntity);

            if (!pageHasCategories) return;

            var sections = await _db.ToListAsync(GetSectionsForPageQuery(page), cancellationToken);

            CopySectionsToPage(page, sections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories for {page}", page.Id);
            throw;
        }
    }

    /// <summary>
    /// Copies the retrieved Sections from the database over the corresponding Category's section
    /// </summary>
    /// <param name="page">Page that contains categories</param>
    /// <param name="sections">"Complete" section from database</param>
    private static void CopySectionsToPage(PageDbEntity page, List<SectionDbEntity> sections)
    {
        var sectionsGroupedByCategory = sections.GroupBy(section => section.CategoryId);

        foreach (var cat in sectionsGroupedByCategory)
        {
            var matching = page.Content.OfType<CategoryDbEntity>()
                                        .FirstOrDefault(category => category != null && category.Id == cat.Key);

            if (matching == null)
            {
                continue;
            }

            matching.Sections = cat.ToList();
        }
    }

    /// <summary>
    /// If the Page.Content has any <see cref="ButtonWithEntryReferenceDbEntity"/>s, load the link to entry reference from the database for it
    /// </summary>
    /// <param name="page"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task TryLoadButtonReferences(PageDbEntity page, CancellationToken cancellationToken)
    {
        try
        {
            var buttons = page.Content.Exists(content => content is ButtonWithEntryReferenceDbEntity);

            if (!buttons) return;

            var buttonQuery = GetButtonWithEntryReferencesQuery(page);

            await _db.ToListAsync(buttonQuery, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading button references for {page}", page.Id);
            throw;
        }
    }

    /// <summary>
    /// Quer to get <see cref="ButtonWithEntryReferenceDbEntity">s for the given page, but with only necessary information we require
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    private IQueryable<ButtonWithEntryReferenceDbEntity> GetButtonWithEntryReferencesQuery(PageDbEntity page)
    => _db.ButtonWithEntryReferences.Where(button => button.ContentPages.Any(contentPage => contentPage.Id == page.Id))
                                    .Select(button => new ButtonWithEntryReferenceDbEntity()
                                    {
                                        Id = button.Id,
                                        LinkToEntry = (IHasSlug)button.LinkToEntry != null ? new PageDbEntity()
                                        {
                                            Slug = ((IHasSlug)button.LinkToEntry).Slug
                                        } : null
                                    });

    /// <summary>
    /// Quer to get <see cref="SectionDbEntity">s for the given page, but with only necessary information we require
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    private IQueryable<SectionDbEntity> GetSectionsForPageQuery(PageDbEntity page)
    => _db.Sections.Where(section => section.Category != null && section.Category.ContentPages.Any(categoryPage => categoryPage.Slug == page.Slug))
                .Where(section => section.Order != null)
                .OrderBy(section => section.Order)
                .Select(section => new SectionDbEntity()
                {
                    CategoryId = section.CategoryId,
                    Id = section.Id,
                    Name = section.Name,
                    Questions = section.Questions.Where(section => section.Order != null).OrderBy(question => question.Order).Select(question => new QuestionDbEntity()
                    {
                        Slug = question.Slug,
                        Id = question.Id,
                    }).ToList(),
                    Recommendations = section.Recommendations.Select(recommendation => new RecommendationPageDbEntity()
                    {
                        DisplayName = recommendation.DisplayName,
                        Maturity = recommendation.Maturity,
                        Page = new PageDbEntity()
                        {
                            Slug = recommendation.Page.Slug
                        }
                    }).ToList(),
                    InterstitialPage = section.InterstitialPage == null ? null : new PageDbEntity()
                    {
                        Slug = section.InterstitialPage.Slug,
                        Id = section.InterstitialPage.Id
                    }
                });

    /// <summary>
    /// Retrieves the page for the given slug from Contentful
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="ContentfulDataUnavailableException"></exception>
    private async Task<Page> GetPageFromContentful(string slug, CancellationToken cancellationToken)
    {
        if (!int.TryParse(_getEntityEnvVariable, out int getEntityValue))
        {
            throw new FormatException($"Could not parse CONTENTFUL_GET_ENTITY_INT environment variable to int. Value: {_getEntityEnvVariable}");
        }

        try
        {
            var options = new GetEntitiesOptions(getEntityValue,
                new[] { new ContentQueryEquals() { Field = "fields.slug", Value = slug } });
            var pages = await repository.GetEntities<Page>(options, cancellationToken);

            var page = pages.FirstOrDefault() ?? throw new KeyNotFoundException($"Could not find page with slug {slug}");

            UpdateSectionTitle(page);

            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching page {slug} from Contentful", slug);
            throw new ContentfulDataUnavailableException($"Could not retrieve page with slug {slug}", ex);
        }
    }

    private void UpdateSectionTitle(Page page)
    {
        if (page.DisplayTopicTitle)
        {
            var cached = _cacher.Cached!;
            page.SectionTitle = cached.CurrentSectionTitle;
        }
    }
}
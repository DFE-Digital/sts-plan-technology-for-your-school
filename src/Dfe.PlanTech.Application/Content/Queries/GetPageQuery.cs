using AutoMapper;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
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

        if (matchingPage == null) return await GetFromContentful(slug, cancellationToken);

        await LoadRichTextContents(matchingPage);

        var mapped = _mapperConfiguration.Map<PageDbEntity, Page>(matchingPage);

        UpdateSectionTitle(mapped);

        return mapped;
    }

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

    private async Task LoadRichTextContents(PageDbEntity page)
    {
        try
        {
            var textBodyContentIds = page.Content.Concat(page.BeforeTitleContent)
                                                .Where(content => content is IHasRichText)
                                                .Select(content => content as IHasRichText)
                                                .Select(content => content!.RichTextId);

            await _db.ToListAsync(_db.LoadRichTextContentsByParentIds(textBodyContentIds));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching rich text content from Database for {pageId}", page.Id);
            throw;
        }
    }

    private async Task<Page> GetFromContentful(string slug, CancellationToken cancellationToken)
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
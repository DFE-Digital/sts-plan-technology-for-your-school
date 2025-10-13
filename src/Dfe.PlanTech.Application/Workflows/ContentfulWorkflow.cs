using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Options;
using Dfe.PlanTech.Core.Contentful.Options;
using Dfe.PlanTech.Core.Contentful.Queries;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Data.Contentful.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Workflows;

public class ContentfulWorkflow(
    ILogger<ContentfulWorkflow> logger,
    IContentfulRepository contentfulRepository,
    GetPageFromContentfulOptions getPageOptions
) : IContentfulWorkflow
{
    public const string ExceptionMessageEntityContentful = "Error fetching Entity from Contentful";
    public const string SlugFieldPath = "fields.interstitialPage.fields.slug";

    private readonly IContentfulRepository _contentfulRepository = contentfulRepository ?? throw new ArgumentNullException(nameof(contentfulRepository));
    private readonly GetPageFromContentfulOptions _getPageOptions = getPageOptions ?? throw new ArgumentNullException(nameof(getPageOptions));

    public async Task<TEntry> GetEntryById<TEntry>(string entryId)
        where TEntry : ContentfulEntry
    {
        try
        {
            var entry = await _contentfulRepository.GetEntryByIdAsync<TEntry>(entryId)
                ?? throw new ContentfulDataUnavailableException($"Could not find entry with ID '{entryId}'");

            return entry;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionMessageEntityContentful);
            throw new ContentfulDataUnavailableException($"Could not find entry with ID '{entryId}'", ex);
        }
    }

    public async Task<List<TEntry>> GetEntries<TEntry>()
        where TEntry : ContentfulEntry
    {
        try
        {
            var entries = await _contentfulRepository.GetEntriesAsync<TEntry>();
            if (!entries.Any())
            {
                throw new ContentfulDataUnavailableException($"Could not find entries of type {typeof(TEntry).Name}");
            }

            return entries.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionMessageEntityContentful);
            throw new ContentfulDataUnavailableException($"Could not find entries of type {typeof(TEntry).Name}", ex);
        }
    }

    public async Task<IEnumerable<QuestionnaireSectionEntry>> GetAllSectionsAsync()
    {
        try
        {
            var options = new GetEntriesOptions(include: 3);
            var sections = await _contentfulRepository.GetEntriesAsync<QuestionnaireSectionEntry>(options);
            return sections;
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException("Error getting sections from Contentful", ex);
        }
    }

    public async Task<string?> GetCategoryHeaderTextBySlugAsync(string slug)
    {
        var contentTypeQuery = new ContentfulQuerySingleValue { Field = "fields.landingPage.sys.contentType.sys.id", Value = "page" };
        var slugQuery = new ContentfulQuerySingleValue { Field = "fields.landingPage.fields.slug", Value = slug };
        var options = new GetEntriesOptions(include: 2, [contentTypeQuery, slugQuery]);

        var categories = await _contentfulRepository.GetEntriesAsync<QuestionnaireCategoryEntry>(options);
        var headerText = categories.FirstOrDefault()?.Header.Text;
        if (headerText is null)
        {
            logger.LogError("Could not find header text for questionnaire category with slug {Slug} from Contentful", slug);
        }

        return headerText;
    }

    public async Task<QuestionnaireCategoryEntry?> GetCategoryBySlugAsync(string slug, int? includeLevel = null)
    {
        var contentTypeQuery = new ContentfulQuerySingleValue { Field = "fields.landingPage.sys.contentType.sys.id", Value = "page" };
        var slugQuery = new ContentfulQuerySingleValue { Field = "fields.landingPage.fields.slug", Value = slug };
        var options = new GetEntriesOptions(includeLevel ?? 5, [contentTypeQuery, slugQuery]);

        var categories = await _contentfulRepository.GetEntriesAsync<QuestionnaireCategoryEntry>(options);
        var category = categories.FirstOrDefault();

        if (category is null)
        {
            logger.LogError("Could not find questionnaire category with slug {Slug} from Contentful", slug);
        }

        return category;
    }

    public async Task<PageEntry> GetPageBySlugAsync(string slug)
    {
        var query = new ContentfulQuerySingleValue { Field = "fields.slug", Value = slug };
        var options = new GetEntriesOptions(_getPageOptions.Include, [query]);

        try
        {
            var pages = await _contentfulRepository.GetEntriesAsync<PageEntry>(options);
            var page = pages.FirstOrDefault();
            if (page is null)
            {
                throw new ContentfulDataUnavailableException($"Could not find a page matching slug '{slug}'");
            }

            return page;
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException($"Error getting page with slug {slug} from Contentful", ex);
        }
    }

    public async Task<int> GetRecommendationChunkCountAsync(int page)
    {
        return await _contentfulRepository.GetEntriesCountAsync<RecommendationChunkEntry>();
    }

    public Task<IEnumerable<RecommendationChunkEntry>> GetPaginatedRecommendationEntriesAsync(int page)
    {
        var options = new GetEntriesOptions(include: 3) { Page = page };
        return _contentfulRepository.GetPaginatedEntriesAsync<RecommendationChunkEntry>(options);
    }

    public async Task<QuestionnaireSectionEntry> GetSectionBySlugAsync(string sectionSlug, int? includeLevel = null)
    {
        var sectionSlugQuery = new ContentfulQuerySingleValue { Field = SlugFieldPath, Value = sectionSlug };
        var contentTypeQuery = new ContentfulQuerySingleValue { Field = "fields.interstitialPage.sys.contentType.sys.id", Value = "page" };
        var options = includeLevel is null
            ? new GetEntriesOptions([sectionSlugQuery, contentTypeQuery])
            : new GetEntriesOptions(includeLevel.Value, [sectionSlugQuery, contentTypeQuery]);

        try
        {
            var sections = await _contentfulRepository.GetEntriesAsync<QuestionnaireSectionEntry>(options);
            var section = sections.FirstOrDefault();
            if (section is null)
            {
                throw new ContentfulDataUnavailableException($"Could not find a section matching slug '{sectionSlug}'");
            }

            return section;
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException($"Error getting section with slug {sectionSlug} from Contentful", ex);
        }
    }
}

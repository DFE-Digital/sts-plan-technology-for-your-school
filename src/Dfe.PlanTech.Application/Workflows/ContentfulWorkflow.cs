using Dfe.PlanTech.Core.Content.Options;
using Dfe.PlanTech.Core.Content.Queries;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Data.Contentful.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Workflows;

public class ContentfulWorkflow(
    ILoggerFactory loggerFactory,
    IContentfulRepository contentfulRepository,
    GetPageFromContentfulOptions getPageOptions
)
{
    public const string ExceptionMessageEntityContentful = "Error fetching Entity from Contentful";
    public const string SlugFieldPath = "fields.interstitialPage.fields.slug";

    private readonly ILogger<ContentfulWorkflow> _logger = loggerFactory.CreateLogger<ContentfulWorkflow>();
    private readonly IContentfulRepository _contentfulRepository = contentfulRepository ?? throw new ArgumentNullException(nameof(contentfulRepository));
    private readonly GetPageFromContentfulOptions _getPageOptions = getPageOptions ?? throw new ArgumentNullException(nameof(getPageOptions));

    public async Task<TDto> GetEntryById<TEntry, TDto>(string entryId)
        where TEntry : IDtoTransformable<TDto>
        where TDto : CmsEntryDto
    {
        try
        {
            var entry = await _contentfulRepository.GetEntryByIdAsync<TEntry>(entryId)
                ?? throw new ContentfulDataUnavailableException($"Could not find entry with ID {entryId}");

            return entry.AsDtoInternal();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessageEntityContentful);
            throw new ContentfulDataUnavailableException($"Could not find entry with ID {entryId}", ex);
        }
    }

    public async Task<List<TDto>> GetEntries<TEntry, TDto>()
        where TEntry : IDtoTransformable<TDto>
        where TDto : CmsEntryDto
    {
        try
        {
            var entries = await _contentfulRepository.GetEntriesAsync<TEntry>()
                ?? throw new ContentfulDataUnavailableException($"Could not find entries of type {typeof(TDto).Name}");

            return entries.Select(e => e.AsDtoInternal()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessageEntityContentful);
            throw new ContentfulDataUnavailableException($"Could not find entries of type {typeof(TDto).Name}", ex);
        }
    }

    public async Task<IEnumerable<CmsQuestionnaireSectionDto>> GetAllSectionsAsync()
    {
        try
        {
            var options = new GetEntriesOptions(include: 3);
            var sections = await _contentfulRepository.GetEntriesAsync<QuestionnaireSectionEntry>(options);
            return sections.Select(s => s.AsDto());
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException("Error getting sections from Contentful", ex);
        }
    }

    public async Task<CmsRecommendationIntroDto?> GetIntroForMaturityAsync(string subtopicId, string maturity)
    {
        var query = new ContentfulQuerySingleValue() { Field = "fields.subtopic.sys.id", Value = subtopicId };
        var options = new GetEntriesOptions(include: 2, [query]);

        options.Select = ["fields.intros", "sys"];

        var subtopicRecommendations = await _contentfulRepository.GetEntriesAsync<SubtopicRecommendationEntry>(options);

        var subtopicRecommendation = subtopicRecommendations.FirstOrDefault();
        if (subtopicRecommendation is null)
        {
            _logger.LogError("Could not find subtopic recommendation in Contentful for subtopic with ID '{SubtopicId}'", subtopicId);
            return null;
        }

        var introForMaturity = subtopicRecommendation.Intros
            .FirstOrDefault(intro => string.Equals(intro.Maturity, maturity, StringComparison.OrdinalIgnoreCase));
        if (introForMaturity is null)
        {
            _logger.LogError("Could not find intro with maturity {Maturity} for subtopic {SubtopicId}", maturity, subtopicId);
            return null;
        }

        return introForMaturity.AsDto();
    }

    public async Task<CmsPageDto> GetPageBySlugAsync(string slug)
    {
        var query = new ContentfulQuerySingleValue { Field = "fields.slug", Value = slug };
        var options = new GetEntriesOptions(_getPageOptions.Include, [query]);

        try
        {
            var pages = await _contentfulRepository.GetEntriesAsync<PageEntry>(options);
            var page = pages.FirstOrDefault();
            if (page is null)
            {
                throw new ContentfulDataUnavailableException($"Could not find a page matching slug '{slug}");
            }

            return page.AsDto();
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException($"Error getting page with slug {slug} from Contentful", ex);
        }
    }

    public async Task<CmsQuestionnaireSectionDto> GetSectionBySlugAsync(string sectionSlug)
    {
        var sectionSlugQuery = new ContentfulQuerySingleValue { Field = SlugFieldPath, Value = sectionSlug };
        var contentTypeQuery = new ContentfulQuerySingleValue { Field = "fields.interstitialPage.sys.contentType.sys.id", Value = "page" };
        var options = new GetEntriesOptions { Queries = [sectionSlugQuery, contentTypeQuery] };

        try
        {
            var sections = await _contentfulRepository.GetEntriesAsync<QuestionnaireSectionEntry>(options);
            var section = sections.FirstOrDefault();
            if (section is null)
            {
                throw new ContentfulDataUnavailableException($"Could not find a section matching slug '{sectionSlug}");
            }

            return section.AsDto();
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException($"Error getting section with slug {sectionSlug} from Contentful", ex);
        }
    }

    public async Task<CmsSubtopicRecommendationDto?> GetSubtopicRecommendationByIdAsync(string subtopicId)
    {
        var sectionIdQuery = new ContentfulQuerySingleValue { Field = "fields.subtopic.sys.id", Value = subtopicId };
        var options = new GetEntriesOptions(4, [sectionIdQuery]);

        var subtopicRecommendations = await _contentfulRepository.GetEntriesAsync<SubtopicRecommendationEntry>(options);
        var subtopicRecommendation = subtopicRecommendations.FirstOrDefault();

        if (subtopicRecommendation is null)
        {
            _logger.LogError("Could not find subtopic recommendation in Contentful for {SubtopicId}", subtopicId);
        }

        return subtopicRecommendation?.AsDto();
    }

    public async Task<CmsRecommendationIntroDto?> GetSubtopicRecommendationIntroByIdAndMaturityAsync(string subtopicId, string maturity)
    {
        var sectionIdQuery = new ContentfulQuerySingleValue { Field = "fields.subtopic.sys.id", Value = subtopicId };
        var options = new GetEntriesOptions(4, [sectionIdQuery]);

        var subtopicRecommendations = await _contentfulRepository.GetEntriesAsync<SubtopicRecommendationEntry>(options);
        var subtopicRecommendation = subtopicRecommendations.FirstOrDefault();

        if (subtopicRecommendation is null)
        {
            _logger.LogError("Could not find subtopic recommendation in Contentful for {SubtopicId}", subtopicId);
            return null;
        }

        var introForMaturity = subtopicRecommendation?.Intros.FirstOrDefault(i => i.Maturity.Equals(maturity));
        if (introForMaturity is null)
        {
            _logger.LogError("Could not find intro with maturity {Maturity} for subtopic {SubtopicId}", maturity, subtopicId);
        }

        return introForMaturity?.AsDto();
    }
}

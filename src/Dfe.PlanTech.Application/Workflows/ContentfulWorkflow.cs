using Dfe.PlanTech.Application.Workflows.ContentfulQueries;
using Dfe.PlanTech.Application.Workflows.Options;
using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Core.Content.Options;
using Dfe.PlanTech.Core.Content.Queries;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Data.Contentful.Persistence;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Workflows
{
    public class ContentfulWorkflow(
        ILogger<ContentfulWorkflow> logger,
        ContentfulRepository contentfulRepository,
        SectionWorkflow sectionEntryRepository
    )
    {
        public const string ExceptionMessageEntityContentful = "Error fetching Entity from Contentful";
        public const string SlugFieldPath = "fields.interstitialPage.fields.slug";

        private readonly ILogger<ContentfulWorkflow> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ContentfulRepository _contentfulRepository = contentfulRepository ?? throw new ArgumentNullException(nameof(contentfulRepository));
        private readonly SectionWorkflow _sectionEntryRepository = sectionEntryRepository ?? throw new ArgumentNullException(nameof(sectionEntryRepository));

        public async Task<TDto?> GetEntryById<TEntry, TDto>(string contentId)
            where TEntry : ContentComponent, new()
            where TDto : CmsEntryDto, new()
        {
            try
            {
                var entry = await _contentfulRepository.GetEntryById<TEntry>(contentId);
                return entry?.AsDto<TDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ExceptionMessageEntityContentful);
                return null;
            }
        }

        public async Task<RecommendationsViewDto?> GetIntroForMaturityAsync(string subtopicId, string maturity)
        {
            var introForMaturity = await _subtopicRecommendationRepository.GetIntroForMaturityAsync(subtopicId, maturity);
            if (introForMaturity is null)
            {
                _logger.LogError("Could not find intro with maturity {Maturity} for subtopic {SubtopicId}", maturity, subtopicId);
            }

            return introForMaturity.AsDto() RecommendationsViewDto(introForMaturity!.Slug, introForMaturity.HeaderText);
        }

        public async Task<CmsQuestionnaireSectionDto?> GetSectionBySlug(string sectionSlug)
        {
            var options = new GetEntriesOptions()
            {
                Queries =
                [
                    new ContentfulQuerySingleValue()
                    {
                        Field = SlugFieldPath,
                        Value = sectionSlug
                    },
                    new ContentfulQuerySingleValue()
                    {
                        Field = "fields.interstitialPage.sys.contentType.sys.id",
                        Value = "page"
                    }
                ]
            };

            try
            {
                var sections = await _contentfulRepository.GetEntries<QuestionnaireSectionEntry>(options);
                return sections.FirstOrDefault()?.Fields.AsDto;
            }
            catch (Exception ex)
            {
                throw new ContentfulDataUnavailableException($"Error getting section with slug {sectionSlug} from Contentful", ex);
            }
        }
    }
}

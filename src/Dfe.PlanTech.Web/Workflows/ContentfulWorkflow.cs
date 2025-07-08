using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories;

namespace Dfe.PlanTech.Application.Workflows
{
    public class ContentfulWorkflow(
        ILogger<ContentfulWorkflow> logger,
        ContentfulRepository contentfulContext,
        SectionEntryRepository sectionEntryRepository,
        SubtopicRecommendationEntryRepository subtopicRecommendationRepository
        )
    {
        public const string ExceptionMessageEntityContentful = "Error fetching Entity from Contentful";

        private readonly ILogger<ContentfulWorkflow> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ContentfulRepository _contentful = contentfulContext ?? throw new ArgumentNullException(nameof(contentfulContext));
        private readonly SectionEntryRepository _sectionEntryRepository = sectionEntryRepository ?? throw new ArgumentNullException(nameof(sectionEntryRepository));
        private readonly SubtopicRecommendationEntryRepository _subtopicRecommendationRepository = subtopicRecommendationRepository ?? throw new ArgumentNullException(nameof(subtopicRecommendationRepository));

        public async Task<TDto?> GetEntryById<TEntry, TDto>(string contentId)
            where TEntry : ContentfulEntry<TDto>, new()
            where TDto : CmsEntryDto, new()
        {
            try
            {
                var entry = await _contentful.GetEntryById<TEntry>(contentId);
                return entry?.AsDto();
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

        public async Task<CmsSectionDto?> GetSectionBySlug(string sectionSlug)
        {
            var section = await _sectionEntryRepository.GetSectionBySlug(sectionSlug);
            return section?.AsDto();
        }
    }
}

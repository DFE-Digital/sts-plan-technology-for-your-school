using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Entries;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories;

namespace Dfe.PlanTech.Web.Workflows
{
    public class ContentfulWorkflow
    {
        public const string ExceptionMessageEntityContentful = "Error fetching Entity from Contentful";

        private readonly ILogger<ContentfulWorkflow> _logger;
        private readonly ContentfulBaseRepository _contentfulRepository;
        private readonly SubtopicRecommendationRepository _subtopicRecommendationRepository;

        public ContentfulWorkflow(
            ILogger<ContentfulWorkflow> logger,
            ContentfulBaseRepository contentfulRepository,
            SubtopicRecommendationRepository subtopicRecommendationRepository
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contentfulRepository = contentfulRepository ?? throw new ArgumentNullException(nameof(contentfulRepository));
            _subtopicRecommendationRepository = subtopicRecommendationRepository ?? throw new ArgumentNullException(nameof(subtopicRecommendationRepository));
        }


        public async Task<TDto?> GetEntryById<TEntry, TDto>(string contentId)
            where TEntry : ContentfulEntry
            where TDto : CmsEntryDto
        {
            try
            {
                var entry = await _contentfulRepository.GetEntryById<TEntry>(contentId);
                return (TDto?)entry?.ToDto();
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

            return new RecommendationsViewDto(introForMaturity!.Slug, introForMaturity.HeaderText);
        }
    }
}

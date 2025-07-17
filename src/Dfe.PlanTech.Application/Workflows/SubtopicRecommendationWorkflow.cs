using Dfe.PlanTech.Core.Content.Options;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories
{
    public class SubtopicRecommendationWorkflow
    {
        private readonly ILogger<SubtopicRecommendationWorkflow> _logger;
        private readonly ContentfulRepository _contentful;

        public SubtopicRecommendationWorkflow(
            ILoggerFactory loggerFactory,
            ContentfulRepository contentfulBaseRepository
        )
        {
            _logger = loggerFactory.CreateLogger<SubtopicRecommendationWorkflow>();
            _contentful = contentfulBaseRepository;
        }

        public async Task<SubtopicRecommendationEntry?> GetFirstSubTopicRecommendationAsync(string subtopicId)
        {
            var options = CreateGetEntityOptions(subtopicId);
            var subTopicRecommendations = await _contentful.GetEntriesAsync<SubtopicRecommendationEntry>(options);

            var subtopicRecommendation = subTopicRecommendations.FirstOrDefault();
            if (subtopicRecommendation is null)
            {
                LogMissingRecommendationError(subtopicId);
            }

            return subtopicRecommendation;
        }

        public async Task<CmsRecommendationDto?> GetIntroForMaturityAsync(string subtopicId, string maturity)
        {
            var options = CreateGetEntityOptions(subtopicId, 2);
            options.Select = ["fields.intros", "sys"];

            var subtopicRecommendations = await _contentful.GetEntriesAsync<SubtopicRecommendationEntry>(options);

            var subtopicRecommendation = subtopicRecommendations.FirstOrDefault();
            if (subtopicRecommendation is null)
            {
                LogMissingRecommendationError(subtopicId);
                return null;
            }

            var introForMaturity = subtopicRecommendation.GetRecommendationByMaturity(maturity);
            if (introForMaturity is null)
            {
                _logger.LogError("Could not find intro with maturity {Maturity} for subtopic {SubtopicId}", maturity, subtopicId);
                return null;
            }

            return introForMaturity.AsDto();
        }

        private static GetEntriesOptions CreateGetEntityOptions(string subtopicId, int depth = 4, params IContentQuery[] additionalQueries) =>
            new(depth, [new ContentfulQuerySingleValue() { Field = "fields.subtopic.sys.id", Value = subtopicId }, .. additionalQueries]);

        private void LogMissingRecommendationError(string subtopicId)
    }
}  

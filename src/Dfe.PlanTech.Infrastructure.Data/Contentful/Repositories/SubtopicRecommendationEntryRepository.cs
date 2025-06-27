using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Entries;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Queries;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories
{
    public class SubtopicRecommendationEntryRepository
    {
        private readonly ILogger<PageEntryRepository> _logger;
        private readonly ContentfulContext _contentful;

        public SubtopicRecommendationEntryRepository(
            ILoggerFactory loggerFactory,
            ContentfulContext contentfulBaseRepository
        )
        {
            _logger = loggerFactory.CreateLogger<PageEntryRepository>();
            _contentful = contentfulBaseRepository;
        }

        public async Task<SubtopicRecommendationEntry?> GetFirstSubTopicRecommendationAsync(string subtopicId)
        {
            var options = CreateGetEntityOptions(subtopicId);
            var subTopicRecommendations = await _contentful.GetEntries<SubtopicRecommendationEntry>(options);

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

            var subtopicRecommendations = await _contentful.GetEntries<SubtopicRecommendationEntry>(options);

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

            return (CmsRecommendationDto)introForMaturity.ToDto();
        }

        private static GetEntriesOptions CreateGetEntityOptions(string subtopicId, int depth = 4, params IContentQuery[] additionalQueries) =>
            new(depth, [new ContentfulQuerySingleValue() { Field = "fields.subtopic.sys.id", Value = subtopicId }, .. additionalQueries]);

        private void LogMissingRecommendationError(string subtopicId)
        => _logger.LogError("Could not find subtopic recommendation in Contentful for subtopic with ID '{SubtopicId}'", subtopicId);
    }
}

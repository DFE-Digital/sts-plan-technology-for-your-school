using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Application.Services
{
    public class RecommendationService(
        RecommendationWorkflow recommendationWorkflow
    )
    {
        private readonly RecommendationWorkflow _recommendationWorkflow = recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));

        public Task<int> GetRecommendationChunkCount(int page)
        {
            return _recommendationWorkflow.GetRecommendationChunkCount(page);
        }

        public Task<IEnumerable<CmsRecommendationChunkDto>> GetPaginatedRecommendationEntries(int page)
        {
            return _recommendationWorkflow.GetPaginatedRecommendationEntries(page);
        }
    }
}

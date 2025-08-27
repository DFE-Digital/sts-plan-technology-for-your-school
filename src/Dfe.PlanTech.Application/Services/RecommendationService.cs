using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Services;

public class RecommendationService(
    IRecommendationWorkflow recommendationWorkflow
)
{
    private readonly IRecommendationWorkflow _recommendationWorkflow = recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));

    public Task<int> GetRecommendationChunkCount(int page)
    {
        return _recommendationWorkflow.GetRecommendationChunkCount(page);
    }

    public Task<IEnumerable<RecommendationChunkEntry>> GetPaginatedRecommendationEntries(int page)
    {
        return _recommendationWorkflow.GetPaginatedRecommendationEntries(page);
    }
}

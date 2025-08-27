using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces
{
    public interface IRecommendationWorkflow
    {
        Task<IEnumerable<RecommendationChunkEntry>> GetPaginatedRecommendationEntries(int page);
        Task<int> GetRecommendationChunkCount(int page);
    }
}
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<IEnumerable<RecommendationChunkEntry>> GetPaginatedRecommendationEntries(int page);
        Task<int> GetRecommendationChunkCount(int page);
    }
}
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces
{
    public interface IRecommendationRepository
    {
        Task<List<RecommendationEntity>> GetRecommendationIdsByContentfulReferencesAsync(IEnumerable<string> recommendationContentfulReferences);
    }
}

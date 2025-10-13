using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IRecommendationRepository
{
    Task<IEnumerable<RecommendationEntity>> GetRecommendationsByContentfulReferencesAsync(IEnumerable<string> recommendationContentfulReferences);
}

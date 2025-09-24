using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IRecommendationRepository
{
    Task<List<RecommendationEntity>> GetRecommendationsByContentfulReferencesAsync(IEnumerable<string> contentfulRefs);
}

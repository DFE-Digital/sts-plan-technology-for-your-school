using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IRecommendationRepository
{
    Task<IEnumerable<RecommendationEntity>> GetRecommendationsByContentfulReferencesAsync(
        IEnumerable<string> recommendationContentfulReferences
    );

    Task<List<RecommendationEntity>> UpsertRecommendations(
        IEnumerable<SqlRecommendationDto> recommendationDtos
    );
}

using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IRecommendationWorkflow
{
    Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesByRecommendationIdAsync(
            IEnumerable<string> recommendationContentfulReferences,
            int establishmentId
        );
}

using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Services;

public class RecommendationService(
    IRecommendationWorkflow recommendationWorkflow
) : IRecommendationService
{
    public Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesByRecommendationIdAsync(
        IEnumerable<string> recommendationContentfulReferences,
        int establishmentId
    )
    {
        return recommendationWorkflow.GetLatestRecommendationStatusesByRecommendationIdAsync(
            recommendationContentfulReferences,
            establishmentId
        );
    }
}

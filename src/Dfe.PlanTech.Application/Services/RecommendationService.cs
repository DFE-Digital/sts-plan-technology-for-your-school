using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Services;

public class RecommendationService(
    IRecommendationWorkflow recommendationWorkflow
) : IRecommendationService
{
    public Task<SqlEstablishmentRecommendationHistoryDto?> GetCurrentRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId)
    {
        return recommendationWorkflow.GetCurrentRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId
        );
    }

    public Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesAsync(
        int establishmentId
    )
    {
        return recommendationWorkflow.GetLatestRecommendationStatusesAsync(
            establishmentId
        );
    }

    public Task UpdateRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId,
        int userId,
        string newStatus,
        string? noteText = null,
        int? matEstablishmentId = null)
    {
        return recommendationWorkflow.UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus,
            noteText,
            matEstablishmentId
        );
    }
}

using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IRecommendationWorkflow
{
    Task<SqlEstablishmentRecommendationHistoryDto?> GetCurrentRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId
    );

    Task<IEnumerable<SqlEstablishmentRecommendationHistoryDto>> GetRecommendationHistoryAsync(
        string recommendationContentfulReference,
        int establishmentId
    );

    Task<
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
    > GetLatestRecommendationStatusesByEstablishmentIdAsync(int establishmentId);

    Task UpdateRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId,
        int userId,
        string newStatus,
        string? noteText = null,
        int? matEstablishmentId = null
    );
    Task<SqlFirstActivityForEstablishmentRecommendationDto?> GetFirstActivityForEstablishmentRecommendationAsync(
        int establishmentId,
        string recommendationContentfulReference
    );
}

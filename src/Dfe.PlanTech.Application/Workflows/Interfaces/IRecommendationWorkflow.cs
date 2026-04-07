using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IRecommendationWorkflow
{
    Task<SqlEstablishmentRecommendationHistoryDto?> GetLatestRecommendationStatusAsync(
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
        RecommendationStatus newStatus,
        string? noteText = null,
        int? matEstablishmentId = null,
        int? responseId = null
    );
    Task<SqlFirstActivityForEstablishmentRecommendationDto?> GetFirstActivityForEstablishmentRecommendationAsync(
        int establishmentId,
        string recommendationContentfulReference
    );
}

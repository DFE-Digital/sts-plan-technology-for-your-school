using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IRecommendationService
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
    > GetLatestRecommendationStatusesAsync(int establishmentId);

    Task UpdateRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId,
        int userId,
        RecommendationStatus newStatus,
        string? noteText = null,
        int? matEstablishmentId = null
    );
    Task<SqlFirstActivityForEstablishmentRecommendationDto?> GetFirstActivityForEstablishmentRecommendationAsync(
        int establishmentId,
        string recommendationContentfulReference
    );
}

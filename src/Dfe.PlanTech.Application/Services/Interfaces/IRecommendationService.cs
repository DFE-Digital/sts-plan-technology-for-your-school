using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IRecommendationService
{
    Task<SqlEstablishmentRecommendationHistoryDto?> GetCurrentRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId
    );

    Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesAsync(
        int establishmentId
    );

    Task UpdateRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId,
        int userId,
        string newStatus,
        string? noteText = null,
        int? matEstablishmentId = null
    );
}

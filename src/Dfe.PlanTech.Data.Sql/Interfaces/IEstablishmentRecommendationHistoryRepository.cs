using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IEstablishmentRecommendationHistoryRepository
{
    Task<
        IEnumerable<EstablishmentRecommendationHistoryEntity>
    > GetRecommendationHistoryByEstablishmentIdAsync(int establishmentId);

    Task<
        IEnumerable<EstablishmentRecommendationHistoryEntity>
    > GetRecommendationHistoryByEstablishmentIdAndRecommendationIdAsync(
        int establishmentId,
        int recommendationId
    );

    Task<EstablishmentRecommendationHistoryEntity?> GetLatestRecommendationHistoryAsync(
        int establishmentId,
        int recommendationId
    );

    Task CreateRecommendationHistoryAsync(
        int establishmentId,
        int recommendationId,
        int userId,
        int? matEstablishmentId,
        string? previousStatus,
        string newStatus,
        string noteText
    );
}

using Dfe.PlanTech.Core.Enums;
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

    Task CreateRecommendationHistoriesAsync(
        int establishmentId,
        int? matEstablishmentId,
        int userId,
        IEnumerable<RecommendationEntity> recommendations,
        IDictionary<string, int> recommendationRefsToResponseIds,
        IDictionary<string, RecommendationStatus> answerStatuses
    );

    Task UpdateRecommendationStatusAsync(
        int establishmentId,
        int recommendationId,
        int userId,
        int? matEstablishmentId,
        RecommendationStatus? previousStatus,
        RecommendationStatus? newStatus,
        string noteText
    );
}

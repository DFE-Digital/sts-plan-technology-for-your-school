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
    > GetRecommendationHistoryByEstablishmentIdAndRecommendationRefAsync(
        int establishmentId,
        string recommendationRef
    );

    Task<EstablishmentRecommendationHistoryEntity?> GetLatestRecommendationHistoryAsync(
        int establishmentId,
        string recommendationRef
    );

    Task CreateRecommendationHistoryAsync(
        int establishmentId,
        int recommendationId,
        int userId,
        int? matEstablishmentId,
        int? responseId,
        RecommendationStatus? previousStatus,
        RecommendationStatus? newStatus,
        string noteText
    );

    Task<
        List<(EstablishmentEntity Establishment, EstablishmentRecommendationHistoryEntity? History)>
    > GetLatestGroupRecommendationHistoryByRecommendationRefAsync(
        int establishmentId,
        string recommendationRef
    );

    Task<List<EstablishmentRecommendationHistoryEntity>> GetLatestRecommendationHistoriesAsync(
        IEnumerable<int> establishmentIds,
        string recommendationRef
    );

    Task CreateRecommendationHistoriesAsync(
        IEnumerable<EstablishmentRecommendationHistoryEntity> histories
    );
}

using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IEstablishmentRecommendationHistoryRepository
{
    Task<IEnumerable<EstablishmentRecommendationHistoryEntity>
    > GetRecommendationHistoryByEstablishmentIdAsync(int establishmentId);

    Task<IEnumerable<EstablishmentRecommendationHistoryEntity>
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
        int? responseId,
        RecommendationStatus? previousStatus,
        RecommendationStatus? newStatus,
        string noteText
    );

    Task<Dictionary<string, int>> GetRecommendationHistoryCountsForEstablishmentsAsync(
    Expression<Func<EstablishmentRecommendationHistoryEntity, bool>> predicate);
    Task<IEnumerable<EstablishmentRecommendationHistoryEntity>> GetRecommendationHistoryForEstablishmentsAsync(Expression<Func<EstablishmentRecommendationHistoryEntity, bool>> predicate);
}

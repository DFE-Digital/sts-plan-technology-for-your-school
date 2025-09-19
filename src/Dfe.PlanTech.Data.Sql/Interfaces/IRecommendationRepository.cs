using System.Linq.Expressions;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IRecommendationRepository
{
    #region Recommendation Management

    /// <summary>
    /// Get a recommendation by its database ID
    /// </summary>
    Task<RecommendationEntity?> GetRecommendationByIdAsync(int id);

    /// <summary>
    /// Get a recommendation by its Contentful reference
    /// </summary>
    Task<RecommendationEntity?> GetRecommendationByContentfulRefAsync(string contentfulRef);

    /// <summary>
    /// Get active (non-archived) recommendations for a specific question
    /// </summary>
    Task<IList<RecommendationEntity>> GetActiveRecommendationsByQuestionIdAsync(int questionId);

    /// <summary>
    /// Generic method to get recommendations using a custom predicate
    /// </summary>
    Task<IList<RecommendationEntity>> GetRecommendationsAsync(Expression<Func<RecommendationEntity, bool>> predicate);

    /// <summary>
    /// Create a new recommendation
    /// </summary>
    Task<RecommendationEntity> CreateRecommendationAsync(RecommendationEntity recommendation);

    /// <summary>
    /// Update an existing recommendation
    /// </summary>
    Task<RecommendationEntity> UpdateRecommendationAsync(RecommendationEntity recommendation);

    /// <summary>
    /// Archive a recommendation (soft delete)
    /// </summary>
    Task ArchiveRecommendationAsync(int id);

    #endregion

    #region Establishment Recommendation Status Management

    /// <summary>
    /// Get the current (latest) status for a specific establishment and recommendation
    /// </summary>
    Task<EstablishmentRecommendationHistoryEntity?> GetCurrentRecommendationStatusAsync(int establishmentId, int recommendationId);

    /// <summary>
    /// Get all current recommendation statuses for an establishment
    /// </summary>
    Task<IList<EstablishmentRecommendationHistoryEntity>> GetCurrentRecommendationStatusesByEstablishmentAsync(int establishmentId);

    /// <summary>
    /// Update the status of a recommendation for an establishment (creates new history record)
    /// TODO/FIXME: Also set the MAT details too, not just the user ID.
    /// </summary>
    Task<EstablishmentRecommendationHistoryEntity> UpdateRecommendationStatusAsync(
        int establishmentId,
        int recommendationId,
        int userId,
        string newStatus,
        string? noteText = null);

    /// <summary>
    /// Generic method to get recommendation status history using a custom predicate
    /// </summary>
    Task<IList<EstablishmentRecommendationHistoryEntity>> GetRecommendationStatusHistoryAsync(Expression<Func<EstablishmentRecommendationHistoryEntity, bool>> predicate);

    #endregion
}

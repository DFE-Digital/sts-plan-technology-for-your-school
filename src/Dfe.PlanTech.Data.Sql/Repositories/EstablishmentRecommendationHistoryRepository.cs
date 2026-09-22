using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class EstablishmentRecommendationHistoryRepository(PlanTechDbContext dbContext)
    : IEstablishmentRecommendationHistoryRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<
        IEnumerable<EstablishmentRecommendationHistoryEntity>
    > GetRecommendationHistoryByEstablishmentIdAsync(int establishmentId)
    {
        return await _db
            .EstablishmentRecommendationHistories.Include(erh => erh.Recommendation)
            .Where(erh => erh.EstablishmentId == establishmentId)
            .ToListAsync();
    }

    public async Task<
        IEnumerable<EstablishmentRecommendationHistoryEntity>
    > GetRecommendationHistoryByEstablishmentIdAndRecommendationIdAsync(
        int establishmentId,
        int recommendationId
    )
    {
        return await _db
            .EstablishmentRecommendationHistories.Where(erh =>
                erh.EstablishmentId == establishmentId && erh.RecommendationId == recommendationId
            )
            .ToListAsync();
    }

    public async Task<EstablishmentRecommendationHistoryEntity?> GetLatestRecommendationHistoryAsync(
        int establishmentId,
        int recommendationId
    )
    {
        return await _db
            .EstablishmentRecommendationHistories.Where(erh =>
                erh.EstablishmentId == establishmentId && erh.RecommendationId == recommendationId
            )
            .OrderByDescending(erh => erh.DateCreated)
            .FirstOrDefaultAsync();
    }

    public async Task CreateRecommendationHistoriesAsync(
        int establishmentId,
        int? matEstablishmentId,
        int userId,
        IEnumerable<RecommendationEntity> recommendations,
        IDictionary<string, int> recommendationRefsToResponseIds,
        IDictionary<string, RecommendationStatus> recommendationStatuses
    )
    {
        var previousStatuses = await _db
            .EstablishmentRecommendationHistories.Where(erh =>
                erh.EstablishmentId == establishmentId
                && erh.MatEstablishmentId == matEstablishmentId
            )
            .GroupBy(erh => erh.Recommendation.ContentfulRef, erh => erh)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.OrderByDescending(erh => erh.DateCreated).First().NewStatus
            );

        var erhEntities = recommendations.Select(
            recommendation => new EstablishmentRecommendationHistoryEntity
            {
                EstablishmentId = establishmentId,
                MatEstablishmentId = matEstablishmentId,
                RecommendationId = recommendation.Id,
                ResponseId = recommendationRefsToResponseIds[recommendation.ContentfulRef],
                UserId = userId,
                PreviousStatus = previousStatuses.TryGetValue(
                    recommendation.ContentfulRef,
                    out var previousStatus
                )
                    ? previousStatus
                    : null,
                NewStatus = recommendationStatuses[recommendation.ContentfulRef],
            }
        );

        await _db.EstablishmentRecommendationHistories.AddRangeAsync(erhEntities);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateRecommendationStatusAsync(
        int establishmentId,
        int recommendationId,
        int userId,
        int? matEstablishmentId,
        RecommendationStatus? previousStatus,
        RecommendationStatus? newStatus,
        string noteText
    )
    {
        var historyEntry = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishmentId,
            RecommendationId = recommendationId,
            UserId = userId,
            MatEstablishmentId = matEstablishmentId,
            ResponseId = null,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            NoteText = noteText,
            DateCreated = DateTime.UtcNow,
        };

        _db.EstablishmentRecommendationHistories.Add(historyEntry);
        await _db.SaveChangesAsync();
    }
}

using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class EstablishmentRecommendationHistoryRepository
    : IEstablishmentRecommendationHistoryRepository
{
    private PlanTechDbContext _db;

    public EstablishmentRecommendationHistoryRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<IEnumerable<EstablishmentRecommendationHistoryEntity>
    > GetRecommendationHistoryByEstablishmentIdAsync(int establishmentId)
    {
        return await _db
            .EstablishmentRecommendationHistories.Include(erh => erh.Recommendation)
            .Where(erh => erh.EstablishmentId == establishmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<EstablishmentRecommendationHistoryEntity>> GetRecommendationHistoryForEstablishmentsAsync(Expression<Func<EstablishmentRecommendationHistoryEntity, bool>> predicate)
    {
        return await _db.EstablishmentRecommendationHistories
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetRecommendationHistoryCountsForEstablishmentsAsync(
    Expression<Func<EstablishmentRecommendationHistoryEntity, bool>> predicate)
    {
        return await _db.EstablishmentRecommendationHistories
            .Where(predicate)
            .Where(x => x.Establishment.EstablishmentRef != null)
            .GroupBy(x => x.Establishment.EstablishmentRef!)
            .Select(g => new
            {
                Urn = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.Urn, x => x.Count);
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

    public async Task CreateRecommendationHistoryAsync(
        int establishmentId,
        int recommendationId,
        int userId,
        int? matEstablishmentId,
        int? responseId,
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
            ResponseId = responseId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            NoteText = noteText,
            DateCreated = DateTime.UtcNow,
        };

        _db.EstablishmentRecommendationHistories.Add(historyEntry);
        await _db.SaveChangesAsync();
    }
}

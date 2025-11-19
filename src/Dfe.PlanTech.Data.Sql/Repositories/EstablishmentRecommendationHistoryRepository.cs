using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class EstablishmentRecommendationHistoryRepository : IEstablishmentRecommendationHistoryRepository
{
    private PlanTechDbContext _db;

    public EstablishmentRecommendationHistoryRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<IEnumerable<EstablishmentRecommendationHistoryEntity>> GetRecommendationHistoryByEstablishmentIdAsync(int establishmentId)
    {
        return await _db.EstablishmentRecommendationHistories
            .Include(erh => erh.Recommendation)
            .Where(erh => erh.EstablishmentId == establishmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<EstablishmentRecommendationHistoryEntity>> GetRecommendationHistoryByEstablishmentIdAndRecommendationIdAsync(int establishmentId, int recommendationId)
    {
        return await _db.EstablishmentRecommendationHistories
            .Where(erh => erh.EstablishmentId == establishmentId && erh.RecommendationId == recommendationId)
            .ToListAsync();
    }

    public async Task<EstablishmentRecommendationHistoryEntity?> GetLatestRecommendationHistoryAsync(int establishmentId, int recommendationId)
    {
        return await _db.EstablishmentRecommendationHistories
            .Where(erh => erh.EstablishmentId == establishmentId && erh.RecommendationId == recommendationId)
            .OrderByDescending(erh => erh.DateCreated)
            .FirstOrDefaultAsync();
    }

    public async Task CreateRecommendationHistoryAsync(
        int establishmentId,
        int recommendationId,
        int userId,
        int? matEstablishmentId,
        string? previousStatus,
        string newStatus,
        string noteText)
    {
        var historyEntry = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishmentId,
            RecommendationId = recommendationId,
            UserId = userId,
            MatEstablishmentId = matEstablishmentId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            NoteText = noteText,
            DateCreated = DateTime.UtcNow
        };

        _db.EstablishmentRecommendationHistories.Add(historyEntry);
        await _db.SaveChangesAsync();
    }
}

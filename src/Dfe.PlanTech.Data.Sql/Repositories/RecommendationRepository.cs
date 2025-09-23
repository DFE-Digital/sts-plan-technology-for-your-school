using System.Linq.Expressions;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class RecommendationRepository : IRecommendationRepository
{
    protected readonly PlanTechDbContext _db;

    public RecommendationRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    #region Recommendation Management

    public async Task<RecommendationEntity?> GetRecommendationByIdAsync(int id)
    {
        return await GetRecommendationsQuery()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<RecommendationEntity?> GetRecommendationByContentfulRefAsync(string contentfulRef)
    {
        return await GetRecommendationsQuery()
            .FirstOrDefaultAsync(r => r.ContentfulRef == contentfulRef);
    }

    public async Task<IList<RecommendationEntity>> GetActiveRecommendationsByQuestionIdAsync(int questionId)
    {
        return await GetRecommendationsQuery()
            .Where(r => r.QuestionId == questionId && !r.Archived)
            .ToListAsync();
    }

    public async Task<IList<RecommendationEntity>> GetRecommendationsAsync(Expression<Func<RecommendationEntity, bool>> predicate)
    {
        return await GetRecommendationsQuery()
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<RecommendationEntity> CreateRecommendationAsync(RecommendationEntity recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation, nameof(recommendation));

        await _db.Recommendations.AddAsync(recommendation);
        await _db.SaveChangesAsync();

        return recommendation;
    }

    public async Task<RecommendationEntity> UpdateRecommendationAsync(RecommendationEntity recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation, nameof(recommendation));

        _db.Recommendations.Update(recommendation);
        await _db.SaveChangesAsync();

        return recommendation;
    }

    public async Task ArchiveRecommendationAsync(int id)
    {
        var recommendation = await _db.Recommendations.FindAsync(id);
        if (recommendation == null)
        {
            throw new InvalidOperationException($"Recommendation not found for ID '{id}'");
        }

        recommendation.Archived = true;
        await _db.SaveChangesAsync();
    }

    #endregion

    #region Establishment Recommendation Status Management

    public async Task<EstablishmentRecommendationHistoryEntity?> GetCurrentRecommendationStatusAsync(int establishmentId, int recommendationId)
    {
        return await GetRecommendationStatusQuery()
            .Where(h => h.EstablishmentId == establishmentId && h.RecommendationId == recommendationId)
            .OrderByDescending(h => h.DateCreated)
            .FirstOrDefaultAsync();
    }

    public async Task<IList<EstablishmentRecommendationHistoryEntity>> GetCurrentRecommendationStatusesByEstablishmentAsync(int establishmentId)
    {
        // Get the latest status for each recommendation for this establishment
        // Use an EF-friendly approach by getting the data in two steps

        // First, get the latest history record IDs for each recommendation
        var latestHistoryIds = await _db.EstablishmentRecommendationHistories
            .Where(h => h.EstablishmentId == establishmentId)
            .GroupBy(h => h.RecommendationId)
            .Select(g => g.OrderByDescending(h => h.DateCreated).First().Id)
            .ToListAsync();

        // Then get the full records with includes
        return await GetRecommendationStatusQuery()
            .Where(h => latestHistoryIds.Contains(h.Id))
            .OrderBy(h => h.Recommendation.QuestionId)
            .ThenBy(h => h.RecommendationId)
            .ToListAsync();
    }

    public async Task<EstablishmentRecommendationHistoryEntity> UpdateRecommendationStatusAsync(
        int establishmentId,
        int recommendationId,
        int userId,
        string newStatus,
        string? noteText = null)
    {
        ArgumentNullException.ThrowIfNull(newStatus, nameof(newStatus));

        // Get the current status to determine the previous status
        var currentStatus = await GetCurrentRecommendationStatusAsync(establishmentId, recommendationId);
        var previousStatus = currentStatus?.NewStatus;


        // TODO/FIXME: Also set the MAT details too, not just the user ID.
        // Always create a new history record (append-only design)
        var newStatusRecord = new EstablishmentRecommendationHistoryEntity
        {
            UserId = userId,
            EstablishmentId = establishmentId,
            MatEstablishmentId = null, // TODO: Placeholder for MAT establishment ID when applicable
            RecommendationId = recommendationId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            NoteText = noteText,
            DateCreated = DateTime.UtcNow
        };

        await _db.EstablishmentRecommendationHistories.AddAsync(newStatusRecord);
        await _db.SaveChangesAsync();

        return newStatusRecord;
    }

    public async Task<IList<EstablishmentRecommendationHistoryEntity>> GetRecommendationStatusHistoryAsync(Expression<Func<EstablishmentRecommendationHistoryEntity, bool>> predicate)
    {
        return await GetRecommendationStatusQuery()
            .Where(predicate)
            .OrderByDescending(h => h.DateCreated)
            .ToListAsync();
    }

    #endregion

    #region Private Helper Methods

    private IQueryable<RecommendationEntity> GetRecommendationsQuery()
    {
        return _db.Recommendations
            .Include(r => r.Question);
    }

    private IQueryable<EstablishmentRecommendationHistoryEntity> GetRecommendationStatusQuery()
    {
        return _db.EstablishmentRecommendationHistories
            .Include(h => h.Establishment)
            .Include(h => h.Recommendation)
                .ThenInclude(r => r.Question)
            .Include(h => h.User)
            .Include(h => h.MatEstablishment);
    }

    #endregion
}

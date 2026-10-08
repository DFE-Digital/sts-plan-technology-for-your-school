using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class EstablishmentRecommendationHistoryRepository
    : IEstablishmentRecommendationHistoryRepository
{
    private PlanTechDbContext _db;

    public EstablishmentRecommendationHistoryRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

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

    public async Task<List<(
            EstablishmentEntity Establishment,
            EstablishmentRecommendationHistoryEntity? History)>>
        GetLatestGroupRecommendationHistoryByRecommendationIdAsync(
            int establishmentId,
            int recommendationId)
    {
        var results = await (
                from activeEstablishment in _db.Establishments

                join link in _db.GiasGroupMemberships
                    on activeEstablishment.GroupUid equals link.GroupUid.ToString()

                join establishment in _db.Establishments
                    on link.Urn.ToString() equals establishment.EstablishmentRef

                where activeEstablishment.Id == establishmentId

                orderby establishment.OrgName

                select new
                {
                    Establishment = establishment,

                    History = _db.EstablishmentRecommendationHistories
                        .Where(history =>
                            history.EstablishmentId == establishment.Id &&
                            history.RecommendationId == recommendationId)
                        .OrderByDescending(history => history.DateCreated)
                        .ThenByDescending(history => history.Id)
                        .FirstOrDefault()
                })
            .AsNoTracking()
            .ToListAsync();

        return results
            .Select(x => (x.Establishment, x.History))
            .ToList();
    }

    public async Task<List<EstablishmentRecommendationHistoryEntity>>
        GetLatestRecommendationHistoriesAsync(
            IEnumerable<int> establishmentIds,
            int recommendationId
        )
    {
        var ids = establishmentIds.ToArray();

        return await _db.EstablishmentRecommendationHistories
            .Where(history =>
                ids.Contains(history.EstablishmentId) &&
                history.RecommendationId == recommendationId
            )
            .GroupBy(history => history.EstablishmentId)
            .Select(group => group
                .OrderByDescending(history => history.DateCreated)
                .ThenByDescending(history => history.Id)
                .First()
            )
            .ToListAsync();
    }

    public async Task CreateRecommendationHistoriesAsync(
        IEnumerable<EstablishmentRecommendationHistoryEntity> histories
    )
    {
        _db.EstablishmentRecommendationHistories.AddRange(histories);

        await _db.SaveChangesAsync();
    }
}

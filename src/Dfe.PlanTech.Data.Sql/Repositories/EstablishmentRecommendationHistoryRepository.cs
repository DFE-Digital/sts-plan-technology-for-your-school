using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class EstablishmentRecommendationHistoryRepository : IEstablishmentRecommendationHistoryRepository
{
    protected readonly PlanTechDbContext _db;

    public EstablishmentRecommendationHistoryRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public Task<List<EstablishmentRecommendationHistoryEntity>> GetRecommendationHistoryByEstablishmentIdAsync(int establishmentId)
    {
        return _db.EstablishmentRecommendationHistories
            .Where(erh => erh.EstablishmentId == establishmentId)
            .ToListAsync();
    }
}

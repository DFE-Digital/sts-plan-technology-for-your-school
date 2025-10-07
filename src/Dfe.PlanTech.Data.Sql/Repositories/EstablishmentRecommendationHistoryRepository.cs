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
            .Where(erh => erh.EstablishmentId == establishmentId)
            .ToListAsync();
    }
}

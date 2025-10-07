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

    public Task<List<RecommendationEntity>> GetRecommendationIdsByContentfulReferencesAsync(IEnumerable<string> recommendationContentfulReferences)
    {
        return _db.Recommendations
            .Where(r => recommendationContentfulReferences.Contains(r.ContentfulRef))
            .ToListAsync();
    }
}

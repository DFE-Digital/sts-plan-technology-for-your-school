using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class RecommendationRepository : IRecommendationRepository
{
    private PlanTechDbContext _db;

    public RecommendationRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<IEnumerable<RecommendationEntity>> GetRecommendationsByContentfulReferencesAsync(
        IEnumerable<string> recommendationContentfulReferences)
    {
        return await _db.Recommendations
            .Where(r => recommendationContentfulReferences.Contains(r.ContentfulRef))
            .ToListAsync();
    }
}

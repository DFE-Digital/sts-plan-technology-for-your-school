using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class RecommendationRepository : IRecommendationRepository
{
    protected readonly PlanTechDbContext _db;

    public RecommendationRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public Task<List<RecommendationEntity>> GetRecommendationsByContentfulReferencesAsync(IEnumerable<string> contentfulRefs)
    {
        return GetRecommendationsByAsync(recommendation => contentfulRefs.Contains(recommendation.ContentfulRef));
    }

    private Task<List<RecommendationEntity>> GetRecommendationsByAsync(Expression<Func<RecommendationEntity, bool>> predicate)
    {
        return _db.Recommendations
            .Where(predicate)
            .Where(recommendation => recommendation != null)
            .ToListAsync();
    }
}

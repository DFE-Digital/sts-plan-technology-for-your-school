using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class RecommendationRepository : IRecommendationRepository
{
    protected readonly PlanTechDbContext _db;

    public RecommendationRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }
}

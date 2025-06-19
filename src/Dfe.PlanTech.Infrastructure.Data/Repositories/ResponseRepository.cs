namespace Dfe.PlanTech.Infrastructure.Data.Repositories;

public class ResponseRepository
{
    protected readonly PlanTechDbContext _db;

    public ResponseRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }
}

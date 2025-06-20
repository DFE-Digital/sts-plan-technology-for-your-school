namespace Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

public class ResponseRepository
{
    protected readonly PlanTechDbContext _db;

    public ResponseRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }
}

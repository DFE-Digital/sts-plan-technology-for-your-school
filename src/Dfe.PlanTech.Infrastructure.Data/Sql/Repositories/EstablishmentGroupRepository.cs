namespace Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

public class EstablishmentGroupRepository
{
    protected readonly PlanTechDbContext _db;

    public EstablishmentGroupRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }
}

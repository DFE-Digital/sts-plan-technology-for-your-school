namespace Dfe.PlanTech.Infrastructure.Data.Repositories;

public class EstablishmentGroupRepository
{
    protected readonly PlanTechDbContext _db;

    public EstablishmentGroupRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }
}

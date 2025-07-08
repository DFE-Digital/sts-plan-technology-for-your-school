using Dfe.PlanTech.Data.Sql;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class EstablishmentGroupRepository
{
    protected readonly PlanTechDbContext _db;

    public EstablishmentGroupRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }
}

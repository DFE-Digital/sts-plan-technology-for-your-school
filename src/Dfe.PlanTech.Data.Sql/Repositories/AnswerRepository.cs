using Dfe.PlanTech.Data.Sql;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class AnswerRepository
{
    protected readonly PlanTechDbContext _db;

    public AnswerRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }
}

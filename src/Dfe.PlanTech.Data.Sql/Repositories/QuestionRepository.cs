namespace Dfe.PlanTech.Data.Sql.Repositories;

public class QuestionRepository
{
    protected readonly PlanTechDbContext _db;

    public QuestionRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }
}

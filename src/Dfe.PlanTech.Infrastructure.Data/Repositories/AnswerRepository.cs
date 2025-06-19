namespace Dfe.PlanTech.Infrastructure.Data.Repositories;

public class AnswerRepository
{
    protected readonly PlanTechDbContext _db;

    public AnswerRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }
}

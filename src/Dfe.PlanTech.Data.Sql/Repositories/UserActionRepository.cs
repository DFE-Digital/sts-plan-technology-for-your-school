using Dfe.PlanTech.Data.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

public class UserActionRepository(PlanTechDbContext dbContext) : IUserActionRepository
{
    public async Task CreateAsync(UserActionEntity userAction)
    {
        dbContext.UserActions.Add(userAction);
        await dbContext.SaveChangesAsync();
    }
}

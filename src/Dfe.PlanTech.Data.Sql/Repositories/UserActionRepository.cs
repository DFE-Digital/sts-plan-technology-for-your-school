using Dfe.PlanTech.Data.Sql;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class UserActionRepository(PlanTechDbContext dbContext) : IUserActionRepository
{
    public async Task CreateAsync(UserActionEntity userAction)
    {
        dbContext.UserActions.Add(userAction);
        await dbContext.SaveChangesAsync();
    }

    public async Task<UserActionEntity?> GetUserActionAsync(Guid id) => await  dbContext.UserActions.FindAsync(id);

}

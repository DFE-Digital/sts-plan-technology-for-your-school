using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class UserActionRepository(PlanTechDbContext dbContext) : IUserActionRepository
{
    private readonly PlanTechDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task CreateAsync(UserActionEntity userAction)
    {
        _dbContext.UserActions.Add(userAction);
        await _dbContext.SaveChangesAsync();
    }

    public Task<int> GetNumberOfTimesBannerViewedByUserAsync(int userId, string bannerId)
    {
        return _dbContext.UserActions.CountAsync(ua =>
            ua.UserId == userId && ua.RequestedUrl.EndsWith(bannerId)
        );
    }
}

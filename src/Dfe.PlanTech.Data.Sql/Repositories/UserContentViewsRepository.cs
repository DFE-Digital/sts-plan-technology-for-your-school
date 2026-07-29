using Dfe.PlanTech.Core.Providers.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class UserContentViewRepository(
    PlanTechDbContext dbContext,
    IUserActionIdProvider userActionIdProvider
) : IUserContentViewRepository
{
    private readonly PlanTechDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IUserActionIdProvider _userActionIdProvider =
        userActionIdProvider ?? throw new ArgumentNullException(nameof(userActionIdProvider));

    public async Task CreateAsync(int userId, string contentfulRef)
    {
        var userActionId = _userActionIdProvider.GetUserActionId();

        var userContentView = new UserContentViewEntity
        {
            UserId = userId,
            ContentfulRef = contentfulRef,
            UserActionId = userActionId,
        };

        _dbContext.UserContentViews.Add(userContentView);
        await _dbContext.SaveChangesAsync();
    }

    public Task<int> GetNumberOfTimesContentViewedByUserAsync(int userId, string contentfulRef)
    {
        return _dbContext.UserContentViews.CountAsync(ua =>
            ua.UserId == userId && ua.ContentfulRef.Equals(contentfulRef)
        );
    }
}

using System.Linq.Expressions;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class UserSettingsRepository : IUserSettingsRepository
{
    protected readonly PlanTechDbContext _db;

    public UserSettingsRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<UserSettingsEntity> UpsertUserSettingsAsync(
        int userId,
        RecommendationSortOrder sortOrder
    )
    {
        var userSettingsEntity = await GetUserSettingsByAsync(u => u.UserId.Equals(userId));

        if (userSettingsEntity is null)
        {
            userSettingsEntity = new UserSettingsEntity
            {
                UserId = userId,
                SortOrder = sortOrder.ToString(),
            };

            await _db.UserSettings.AddAsync(userSettingsEntity);
        }
        else
        {
            userSettingsEntity.SortOrder = sortOrder.ToString();
        }

        await _db.SaveChangesAsync();

        return userSettingsEntity;
    }

    public Task<UserSettingsEntity?> GetUserSettingsByUserIdAsync(int userId)
    {
        return GetUserSettingsByAsync(u => u.UserId.Equals(userId));
    }

    private Task<UserSettingsEntity?> GetUserSettingsByAsync(
        Expression<Func<UserSettingsEntity, bool>> predicate
    )
    {
        return _db.UserSettings.FirstOrDefaultAsync(predicate);
    }
}

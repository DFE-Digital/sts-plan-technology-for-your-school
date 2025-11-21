using System.Linq.Expressions;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
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

    public async Task<SqlUserSettingsDto> UpsertUserSettings(int userId, RecommendationSortOrder sortOrder)
    {
        var userSettingsEntity = await GetUserSettingsByAsync(u => u.UserId.Equals(userId));

        if (userSettingsEntity is null)
        {
            userSettingsEntity = new UserSettingsEntity
            {
                UserId = userId,
                SortOrder = sortOrder.ToString()
            };

            await _db.UserSettings.AddAsync(userSettingsEntity);
        }
        else
        {
            userSettingsEntity.SortOrder = sortOrder.ToString();
        }

        await _db.SaveChangesAsync();

        return userSettingsEntity.AsDto();
    }

    public async Task<SqlUserSettingsDto?> GetUserSettingsByUserId(int userId)
    {
        var userSettings = await GetUserSettingsByAsync(u => u.UserId.Equals(userId));
        return userSettings?.AsDto();
    }

    private Task<UserSettingsEntity?> GetUserSettingsByAsync(Expression<Func<UserSettingsEntity, bool>> predicate)
    {
        return _db.UserSettings
            .FirstOrDefaultAsync(predicate);
    }
}

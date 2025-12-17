using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces
{
    public interface IUserSettingsRepository
    {
        Task<UserSettingsEntity> UpsertUserSettingsAsync(int userId, RecommendationSortOrder sortOrder);
        Task<UserSettingsEntity?> GetUserSettingsByUserIdAsync(int userId);
    }
}

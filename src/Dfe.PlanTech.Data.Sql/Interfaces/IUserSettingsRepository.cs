using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces
{
    public interface IUserSettingsRepository
    {
        Task<UserSettingsEntity?> GetUserSettingsByUserIdAsync(int userId);
        Task<UserSettingsEntity> SetUserRecommendationSortOrderAsync(
            int userId,
            RecommendationSortOrder sortOrder
        );
    }
}

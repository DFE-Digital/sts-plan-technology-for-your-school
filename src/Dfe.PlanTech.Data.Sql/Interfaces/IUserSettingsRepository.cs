using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Data.Sql.Interfaces
{
    public interface IUserSettingsRepository
    {
        Task<SqlUserSettingsDto> UpsertUserSettings(int userId, RecommendationSortOrder sortOrder);
        Task<SqlUserSettingsDto?> GetUserSettingsByUserId(int userId);
    }
}

using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<SqlUserSettingsDto?> GetUserSettingsByUserIdAsync(int userId);
        Task<SqlUserSettingsDto> SetUserRecommendationSortOrderAsync(
            int userId,
            RecommendationSortOrder sortOrder
        );
    }
}

using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IUserWorkflow
{
    Task<SqlUserDto?> GetUserBySignInRefAsync(string dfeSignInRef);
    Task<SqlUserSettingsDto> UpsertUserSettingsAsync(int userId, RecommendationSortOrder sortOrder);
    Task<SqlUserSettingsDto?> GetUserSettingsByUserIdAsync(int userId);
}

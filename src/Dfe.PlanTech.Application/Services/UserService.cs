using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Application.Services;

public class UserService(
    IUserWorkflow userWorkflow
) : IUserService
{
    private readonly IUserWorkflow _userWorkflow = userWorkflow ?? throw new ArgumentNullException(nameof(userWorkflow));

    public Task<SqlUserSettingsDto> UpsertUserSettingsAsync(int userId, RecommendationSortOrder sortOrder)
    {
        return _userWorkflow.UpsertUserSettingsAsync(userId, sortOrder);
    }

    public Task<SqlUserSettingsDto?> GetUserSettingsByUserIdAsync(int userId)
    {
        return _userWorkflow.GetUserSettingsByUserIdAsync(userId);
    }
}

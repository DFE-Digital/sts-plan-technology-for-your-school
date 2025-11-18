using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class UserWorkflow(
    IUserRepository userRepository,
    IUserSettingsRepository userSettingsRepository
) : IUserWorkflow
{
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IUserSettingsRepository _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));

    public async Task<SqlUserDto?> GetUserBySignInRefAsync(string dfeSignInRef)
    {
        var user = await _userRepository.GetUserBySignInRefAsync(dfeSignInRef);
        return user?.AsDto();
    }

    public Task<SqlUserSettingsDto> UpsertUserSettings(int userId, RecommendationSortOrder sortOrder)
    {
        return _userSettingsRepository.UpsertUserSettings(userId, sortOrder);
    }

    public Task<SqlUserSettingsDto?> GetUserSettingsByUserIdAsync(int userId)
    {
        return _userSettingsRepository.GetUserSettingsByUserId(userId);
    }
}


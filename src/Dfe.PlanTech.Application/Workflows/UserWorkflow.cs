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

    public async Task<SqlUserSettingsDto> UpsertUserSettingsAsync(int userId, RecommendationSortOrder sortOrder)
    {
        var userSettings = await _userSettingsRepository.UpsertUserSettingsAsync(userId, sortOrder);
        return userSettings.AsDto();
    }

    public async Task<SqlUserSettingsDto?> GetUserSettingsByUserIdAsync(int userId)
    {
        var userSettings = await _userSettingsRepository.GetUserSettingsByUserIdAsync(userId);
        return userSettings?.AsDto();

    }
}


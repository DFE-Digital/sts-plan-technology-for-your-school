using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Enums;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class UserServiceTests
{
    private readonly IUserWorkflow _userWorkflow = Substitute.For<IUserWorkflow>();

    private UserService CreateServiceUnderTest() => new UserService(_userWorkflow);

    [Fact]
    public async Task UpsertUserSettings_Calls_Workflow()
    {
        var sut = CreateServiceUnderTest();

        var result = await sut.UpsertUserSettingsAsync(123, RecommendationSortOrder.LastUpdated);

        await _userWorkflow.Received(1).UpsertUserSettingsAsync(123, RecommendationSortOrder.LastUpdated);
    }

    [Fact]
    public async Task GetUserSettingsByUserIdAsync_Calls_Workflow()
    {
        var sut = CreateServiceUnderTest();

        var result = await sut.GetUserSettingsByUserIdAsync(123);

        await _userWorkflow.Received(1).GetUserSettingsByUserIdAsync(123);
    }
}

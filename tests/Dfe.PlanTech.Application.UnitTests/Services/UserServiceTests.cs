using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Tests.Shared.Assertions;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class UserServiceTests
{
    private readonly IUserWorkflow _userWorkflow = Substitute.For<IUserWorkflow>();

    private UserService CreateSystemUnderTest() => new(_userWorkflow);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<UserService>(_userWorkflow);
    }

    [Fact]
    public async Task GetUserSettingsByUserIdAsync_WhenCalled_ReturnsWorkflowResult()
    {
        // Arrange
        var expectedSettings = new SqlUserSettingsDto
        {
            UserId = 42,
            SortOrder = RecommendationSortOrder.LastUpdated,
        };
        _userWorkflow.GetUserSettingsByUserIdAsync(42).Returns(expectedSettings);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetUserSettingsByUserIdAsync(42);

        // Assert
        Assert.Same(expectedSettings, result);
    }

    [Fact]
    public async Task SetUserRecommendationSortOrderAsync_WhenCalled_ReturnsWorkflowResult()
    {
        // Arrange
        var expectedSettings = new SqlUserSettingsDto
        {
            UserId = 42,
            SortOrder = RecommendationSortOrder.LastUpdated,
        };
        _userWorkflow
            .SetUserRecommendationSortOrderAsync(42, RecommendationSortOrder.LastUpdated)
            .Returns(expectedSettings);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SetUserRecommendationSortOrderAsync(
            42,
            RecommendationSortOrder.LastUpdated
        );

        // Assert
        Assert.Same(expectedSettings, result);
    }
}

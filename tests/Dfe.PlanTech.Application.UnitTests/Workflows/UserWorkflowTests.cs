using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class UserWorkflowTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUserSettingsRepository _userSettingsRepository =
        Substitute.For<IUserSettingsRepository>();

    private UserWorkflow CreateSystemUnderTest() => new(_userRepository, _userSettingsRepository);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<UserWorkflow>(
            _userRepository,
            _userSettingsRepository
        );
    }

    [Fact]
    public async Task GetUserBySignInRefAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        _userRepository.GetUserBySignInRefAsync("dsi-user-42").Returns((UserEntity?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetUserBySignInRefAsync("dsi-user-42");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserBySignInRefAsync_WhenUserExists_ReturnsMappedUser()
    {
        // Arrange
        var user = new UserEntityBuilder().WithDfeSignInRef("dsi-user-42").Build();
        user.Id = 42;
        _userRepository.GetUserBySignInRefAsync("dsi-user-42").Returns(user);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetUserBySignInRefAsync("dsi-user-42");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("dsi-user-42", result.DfeSignInRef);
    }

    [Fact]
    public async Task GetOrCreateUserAsync_WhenUserExists_ReturnsExistingUserWithoutCreating()
    {
        // Arrange
        var user = new UserEntityBuilder().WithDfeSignInRef("dsi-user-42").Build();
        user.Id = 42;
        _userRepository.GetUserBySignInRefAsync("dsi-user-42").Returns(user);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetOrCreateUserAsync("dsi-user-42");

        // Assert
        Assert.Equal(42, result.Id);
        await _userRepository.DidNotReceive().CreateUserBySignInRefAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetOrCreateUserAsync_WhenUserDoesNotExist_CreatesAndReturnsNewUser()
    {
        // Arrange
        var createdUser = new UserEntityBuilder().WithDfeSignInRef("dsi-user-43").Build();
        createdUser.Id = 43;
        _userRepository.GetUserBySignInRefAsync("dsi-user-43").Returns((UserEntity?)null);
        _userRepository.CreateUserBySignInRefAsync("dsi-user-43").Returns(createdUser);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetOrCreateUserAsync("dsi-user-43");

        // Assert
        Assert.Equal(43, result.Id);
        Assert.Equal("dsi-user-43", result.DfeSignInRef);
        await _userRepository.Received(1).CreateUserBySignInRefAsync("dsi-user-43");
    }

    [Fact]
    public async Task GetUserSettingsByUserIdAsync_WhenSettingsDoNotExist_ReturnsNull()
    {
        // Arrange
        _userSettingsRepository
            .GetUserSettingsByUserIdAsync(42)
            .Returns((UserSettingsEntity?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetUserSettingsByUserIdAsync(42);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserSettingsByUserIdAsync_WhenSettingsExist_ReturnsMappedSettings()
    {
        // Arrange
        _userSettingsRepository
            .GetUserSettingsByUserIdAsync(42)
            .Returns(
                new UserSettingsEntity
                {
                    UserId = 42,
                    SortOrder = nameof(RecommendationSortOrder.LastUpdated),
                }
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetUserSettingsByUserIdAsync(42);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.UserId);
        Assert.Equal(RecommendationSortOrder.LastUpdated, result.SortOrder);
    }

    [Fact]
    public async Task SetUserRecommendationSortOrderAsync_WhenCalled_ReturnsMappedSettings()
    {
        // Arrange
        _userSettingsRepository
            .SetUserRecommendationSortOrderAsync(42, RecommendationSortOrder.LastUpdated)
            .Returns(
                new UserSettingsEntity
                {
                    UserId = 42,
                    SortOrder = nameof(RecommendationSortOrder.LastUpdated),
                }
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SetUserRecommendationSortOrderAsync(
            42,
            RecommendationSortOrder.LastUpdated
        );

        // Assert
        Assert.Equal(42, result.UserId);
        Assert.Equal(RecommendationSortOrder.LastUpdated, result.SortOrder);
    }
}

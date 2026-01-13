using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class UserWorkflowTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUserSettingsRepository _userSettingsRepository = Substitute.For<IUserSettingsRepository>();

    private UserWorkflow CreateServiceUnderTest() => new UserWorkflow(_userRepository, _userSettingsRepository);

    // --- ctor guard ---
    [Fact]
    public void Ctor_NullUserRepository_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UserWorkflow(null!, _userSettingsRepository));
    }

    [Fact]
    public void Ctor_NullUserSettingsRepository_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UserWorkflow(_userRepository, null!));
    }

    // --- GetUserBySignInRefAsync ---

    [Fact]
    public async Task GetUserBySignInRefAsync_Returns_Null_When_Not_Found()
    {
        var sut = CreateServiceUnderTest();
        _userRepository.GetUserBySignInRefAsync("dsi-123").Returns((UserEntity?)null);

        var result = await sut.GetUserBySignInRefAsync("dsi-123");

        Assert.Null(result);
        await _userRepository.Received(1).GetUserBySignInRefAsync("dsi-123");
    }

    [Fact]
    public async Task GetUserBySignInRefAsync_Returns_Mapped_Dto_When_Found()
    {
        var sut = CreateServiceUnderTest();
        var entity = new UserEntity { Id = 42, DfeSignInRef = "dsi-abc" };
        _userRepository.GetUserBySignInRefAsync("dsi-abc").Returns(entity);

        var dto = await sut.GetUserBySignInRefAsync("dsi-abc");

        Assert.NotNull(dto);
        Assert.Equal(42, dto!.Id);
        Assert.Equal("dsi-abc", dto.DfeSignInRef);
        await _userRepository.Received(1).GetUserBySignInRefAsync("dsi-abc");
    }

    // --- UpsertUserSettingsAsync ---

    [Fact]
    public async Task UpsertUserSettings_CallsRepository()
    {
        var sut = CreateServiceUnderTest();

        _userSettingsRepository.UpsertUserSettingsAsync(123, Arg.Any<RecommendationSortOrder>())
            .Returns(new UserSettingsEntity { UserId = 123, SortOrder = nameof(RecommendationSortOrder.LastUpdated) });

        var result = await sut.UpsertUserSettingsAsync(123, RecommendationSortOrder.LastUpdated);

        await _userSettingsRepository.Received(1).UpsertUserSettingsAsync(123, RecommendationSortOrder.LastUpdated);
    }

    // --- GetUserSettingsByUserIdAsync ---

    [Fact]
    public async Task GetUserSettingsByUserIdAsync_Returns_Null_When_Not_Found()
    {
        var sut = CreateServiceUnderTest();
        _userSettingsRepository.GetUserSettingsByUserIdAsync(123).Returns((UserSettingsEntity?)null);

        var result = await sut.GetUserSettingsByUserIdAsync(123);

        Assert.Null(result);
        await _userSettingsRepository.Received(1).GetUserSettingsByUserIdAsync(123);
    }

    [Fact]
    public async Task GetUserSettingsByUserIdAsync_Returns_Mapped_Dto_When_Found()
    {
        var sut = CreateServiceUnderTest();
        var entity = new UserSettingsEntity { UserId = 42, SortOrder = RecommendationSortOrder.LastUpdated.GetDisplayName() };
        _userSettingsRepository.GetUserSettingsByUserIdAsync(42).Returns(entity);

        var dto = await sut.GetUserSettingsByUserIdAsync(42);

        Assert.NotNull(dto);
        Assert.Equal(42, dto!.UserId);
        Assert.Equal(RecommendationSortOrder.LastUpdated, dto.SortOrder);
        await _userSettingsRepository.Received(1).GetUserSettingsByUserIdAsync(42);
    }
}

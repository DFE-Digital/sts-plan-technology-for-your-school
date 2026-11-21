using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class UserContentViewServiceTests
{
    private readonly FakeLogger<UserActionTrackingService> _logger = new();
    private readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();
    private readonly IUserContentViewRepository _userContentViewRepository =
        Substitute.For<IUserContentViewRepository>();

    private UserContentViewService CreateSystemUnderTest() =>
        new(_logger, _currentUserProvider, _userContentViewRepository);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<UserContentViewService>(
            _logger,
            _currentUserProvider,
            _userContentViewRepository
        );
    }

    [Fact]
    public async Task RecordContentViewAsync_WhenCalled_SavesViewForCurrentUser()
    {
        // Arrange
        _currentUserProvider.UserId.Returns(42);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RecordContentViewAsync("banner-ref");

        // Assert
        await _userContentViewRepository.Received(1).CreateAsync(42, "banner-ref");
    }

    [Fact]
    public async Task GetNumberOfTimesContentViewedByUserAsync_WhenCalled_ReturnsRepositoryCountForCurrentUser()
    {
        // Arrange
        _currentUserProvider.UserId.Returns(42);
        _userContentViewRepository
            .GetNumberOfTimesContentViewedByUserAsync(42, "banner-ref")
            .Returns(3);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetNumberOfTimesContentViewedByUserAsync("banner-ref");

        // Assert
        Assert.Equal(3, result);
    }
}

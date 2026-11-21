using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Context;

public class UserActionTrackingServiceTests
{
    private const int UserId = 101;
    private const int UserEstablishmentId = 201;
    private const int FocusedEstablishmentId = 301;
    private const string RequestPath = "/test-path";
    private const string RequestQueryString = "?id=1";
    private const string ExpectedRequestedUrl = "/test-path?id=1";

    private static readonly Guid SessionId = new("5f1c1a4e-3c0f-4d0a-9a53-0a8f2f0c2b11");
    private static readonly Guid ExistingUserActionId = new("0b6a5c1d-7a79-4d57-8f6e-9f5b8f7f1d22");

    private readonly FakeLogger<UserActionTrackingService> _logger = new();
    private readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();
    private readonly IHttpContextAccessor _httpContextAccessor =
        Substitute.For<IHttpContextAccessor>();
    private readonly IUserActionRepository _userActionRepository =
        Substitute.For<IUserActionRepository>();
    private readonly DefaultHttpContext _httpContext = new();

    private UserActionTrackingService CreateSystemUnderTest()
    {
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        return new UserActionTrackingService(
            _logger,
            _currentUserProvider,
            _httpContextAccessor,
            _userActionRepository
        );
    }

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<UserActionTrackingService>(
            _logger,
            _currentUserProvider,
            _httpContextAccessor,
            _userActionRepository
        );
    }

    [Fact]
    public async Task RecordActionAsync_WhenUserIsNotTrust_CreatesUserActionForUserEstablishment()
    {
        // Arrange
        _currentUserProvider.UserId.Returns(UserId);
        _currentUserProvider.SessionId.Returns(SessionId);
        _currentUserProvider.IsTrust.Returns(false);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(UserEstablishmentId).Build());
        _httpContext.Request.Path = RequestPath;
        _httpContext.Request.QueryString = new QueryString(RequestQueryString);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RecordActionAsync();

        // Assert
        await _userActionRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<UserActionEntity>(entity =>
                    entity.Id != Guid.Empty
                    && entity.SessionId == SessionId
                    && entity.UserId == UserId
                    && entity.EstablishmentId == UserEstablishmentId
                    && entity.MatEstablishmentId == null
                    && entity.RequestedUrl == ExpectedRequestedUrl
                )
            );
    }

    [Fact]
    public async Task RecordActionAsync_WhenUserIsNotTrust_DoesNotReadFocusedEstablishment()
    {
        // Arrange
        _currentUserProvider.UserId.Returns(UserId);
        _currentUserProvider.IsTrust.Returns(false);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(UserEstablishmentId).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RecordActionAsync();

        // Assert
        await _currentUserProvider.DidNotReceive().GetFocusedEstablishmentAsync();
    }

    [Fact]
    public async Task RecordActionAsync_WhenUserIsTrust_CreatesUserActionForFocusedEstablishmentAndTrust()
    {
        // Arrange
        _currentUserProvider.UserId.Returns(UserId);
        _currentUserProvider.IsTrust.Returns(true);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(UserEstablishmentId).Build());
        _currentUserProvider
            .GetFocusedEstablishmentAsync()
            .Returns(
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FocusedEstablishmentId)
                    .Build()
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RecordActionAsync();

        // Assert
        await _userActionRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<UserActionEntity>(entity =>
                    entity.EstablishmentId == FocusedEstablishmentId
                    && entity.MatEstablishmentId == UserEstablishmentId
                )
            );
    }

    [Fact]
    public async Task RecordActionAsync_WhenTrustUserHasNoFocusedEstablishment_CreatesUserActionWithoutEstablishment()
    {
        // Arrange
        _currentUserProvider.UserId.Returns(UserId);
        _currentUserProvider.IsTrust.Returns(true);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(UserEstablishmentId).Build());
        _currentUserProvider
            .GetFocusedEstablishmentAsync()
            .Returns((SelectedEstablishmentDataModel?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RecordActionAsync();

        // Assert
        await _userActionRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<UserActionEntity>(entity =>
                    entity.EstablishmentId == null
                    && entity.MatEstablishmentId == UserEstablishmentId
                )
            );
    }

    [Fact]
    public async Task RecordActionAsync_WhenCalled_StoresUserActionIdAndRecordedFlagInHttpContextItems()
    {
        // Arrange
        _currentUserProvider.UserId.Returns(UserId);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(UserEstablishmentId).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RecordActionAsync();

        // Assert
        var userActionId = Assert.IsType<Guid>(
            _httpContext.Items[UserActionIdConstants.HttpContextItemKey]
        );
        Assert.NotEqual(Guid.Empty, userActionId);
        Assert.Equal(true, _httpContext.Items[UserActionIdConstants.RecordedHttpContextItemKey]);
    }

    [Fact]
    public async Task RecordActionAsync_WhenMiddlewareHasCreatedUserActionId_UsesExistingId()
    {
        // Arrange
        _httpContext.Items[UserActionIdConstants.HttpContextItemKey] = ExistingUserActionId;
        _currentUserProvider.UserId.Returns(UserId);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(UserEstablishmentId).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RecordActionAsync();

        // Assert
        await _userActionRepository
            .Received(1)
            .CreateAsync(Arg.Is<UserActionEntity>(entity => entity.Id == ExistingUserActionId));
        Assert.Equal(
            ExistingUserActionId,
            _httpContext.Items[UserActionIdConstants.HttpContextItemKey]
        );
    }

    [Fact]
    public async Task RecordActionAsync_WhenActionAlreadyRecorded_DoesNotCreateAnotherUserAction()
    {
        // Arrange
        _httpContext.Items[UserActionIdConstants.HttpContextItemKey] = ExistingUserActionId;
        _httpContext.Items[UserActionIdConstants.RecordedHttpContextItemKey] = true;
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RecordActionAsync();

        // Assert
        await _userActionRepository.DidNotReceive().CreateAsync(Arg.Any<UserActionEntity>());
    }

    [Fact]
    public async Task RecordActionAsync_WhenHttpContextIsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.RecordActionAsync()
        );
        Assert.Equal("No active HttpContext found.", exception.Message);
    }

    [Fact]
    public async Task GetAsync_WhenUserActionDoesNotExist_ReturnsNull()
    {
        // Arrange
        _userActionRepository
            .GetUserActionAsync(ExistingUserActionId)
            .Returns(Task.FromResult<UserActionEntity?>(null));
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetAsync(ExistingUserActionId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenUserActionExists_ReturnsMappedDto()
    {
        // Arrange
        var userActionEntity = new UserActionEntity
        {
            Id = ExistingUserActionId,
            SessionId = SessionId,
            UserId = 123,
            EstablishmentId = 456,
            MatEstablishmentId = 789,
            RequestedUrl = "/test-path?section=network",
        };
        _userActionRepository
            .GetUserActionAsync(ExistingUserActionId)
            .Returns(Task.FromResult<UserActionEntity?>(userActionEntity));
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetAsync(ExistingUserActionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExistingUserActionId, result.Id);
        Assert.Equal(SessionId, result.SessionId);
        Assert.Equal(123, result.UserId);
        Assert.Equal(456, result.EstablishmentId);
        Assert.Equal(789, result.MatEstablishmentId);
        Assert.Equal("/test-path?section=network", result.RequestedUrl);
    }
}

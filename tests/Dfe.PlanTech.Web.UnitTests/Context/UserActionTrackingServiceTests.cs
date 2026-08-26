using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Context;

public class UserActionTrackingServiceTests
{
    private readonly IUserActionRepository _userActionRepository =
        Substitute.For<IUserActionRepository>();
    private readonly IHttpContextAccessor _httpContextAccessor =
        Substitute.For<IHttpContextAccessor>();
    private readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();
    private readonly ILogger<UserActionTrackingService> _logger = Substitute.For<
        ILogger<UserActionTrackingService>
    >();
    private readonly DefaultHttpContext _httpContext = new();

    private UserActionTrackingService BuildService()
    {
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        return new UserActionTrackingService(
            _logger,
            _currentUser,
            _httpContextAccessor,
            _userActionRepository
        );
    }

    [Fact]
    public async Task RecordAsync_WhenCalled_ThenCreatesUserActionAndStoresIdInHttpContextItems()
    {
        _currentUser.UserId.Returns(101);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(201);
        _currentUser.IsMat.Returns(false);

        _httpContext.Request.Path = "/test-path";
        _httpContext.Request.QueryString = new QueryString("?id=1");

        var service = BuildService();

        await service.RecordActionAsync();

        await _userActionRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<UserActionEntity>(entity =>
                    entity.Id != Guid.Empty
                    && entity.UserId == 101
                    && entity.EstablishmentId == 201
                    && entity.MatEstablishmentId == null
                    && entity.RequestedUrl == "/test-path?id=1"
                )
            );

        Assert.True(_httpContext.Items.ContainsKey(UserActionIdConstants.HttpContextItemKey));
        Assert.IsType<Guid>(_httpContext.Items[UserActionIdConstants.HttpContextItemKey]);
        Assert.True(_httpContext.Items.ContainsKey(UserActionIdConstants.RecordedHttpContextItemKey));
        Assert.True(Assert.IsType<bool>(_httpContext.Items[UserActionIdConstants.RecordedHttpContextItemKey]));
    }

    [Fact]
    public async Task RecordAsync_WhenCurrentUserIsMat_ThenCreatesUserActionWithMatEstablishmentId()
    {
        _currentUser.UserId.Returns(101);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(201);
        _currentUser.IsMat.Returns(true);
        _currentUser.UserOrganisationId.Returns(301);

        var service = BuildService();

        await service.RecordActionAsync();

        await _userActionRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<UserActionEntity>(entity =>
                    entity.EstablishmentId == 201 && entity.MatEstablishmentId == 301
                )
            );
    }

    [Fact]
    public async Task RecordAsync_WhenUserIdIsNull_ThenDoesNotCreateUserAction()
    {
        _currentUser.UserId.Returns((int?)null);

        var service = BuildService();

        await service.RecordActionAsync();

        await _userActionRepository.DidNotReceive().CreateAsync(Arg.Any<UserActionEntity>());
        Assert.False(_httpContext.Items.ContainsKey(UserActionIdConstants.HttpContextItemKey));
    }

    [Fact]
    public async Task RecordAsync_WhenHttpContextIsNull_ThenThrowsInvalidOperationException()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var service = new UserActionTrackingService(
            _logger,
            _currentUser,
            _httpContextAccessor,
            _userActionRepository
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RecordActionAsync()
        );

        Assert.Equal("No active HttpContext found.", exception.Message);
    }

    [Fact]
    public async Task GetAsync_Returns_Null_If_UserAction_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        _userActionRepository
            .GetUserActionAsync(id)
            .Returns(Task.FromResult<UserActionEntity?>(null));

        var sut = BuildService();

        // Act
        var result = await sut.GetAsync(id);

        // Assert
        Assert.Null(result);

        await _userActionRepository.Received(1).GetUserActionAsync(id);
    }

    [Fact]
    public async Task GetAsync_Maps_UserActionEntity_To_Dto()
    {
        // Arrange
        var id = Guid.NewGuid();

        var userActionEntity = new UserActionEntity
        {
            Id = id,
            SessionId = Guid.NewGuid(),
            UserId = 123,
            EstablishmentId = 456,
            MatEstablishmentId = 789,
            RequestedUrl = "/test-path?section=network",
        };

        _userActionRepository
            .GetUserActionAsync(id)
            .Returns(Task.FromResult<UserActionEntity?>(userActionEntity));

        var sut = BuildService();

        // Act
        var result = await sut.GetAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userActionEntity.Id, result.Id);
        Assert.Equal(userActionEntity.SessionId, result.SessionId);
        Assert.Equal(userActionEntity.UserId, result.UserId);
        Assert.Equal(userActionEntity.EstablishmentId, result.EstablishmentId);
        Assert.Equal(userActionEntity.MatEstablishmentId, result.MatEstablishmentId);
        Assert.Equal(userActionEntity.RequestedUrl, result.RequestedUrl);

        await _userActionRepository.Received(1).GetUserActionAsync(id);
    }

    [Fact]
    public async Task RecordAsync_WhenMiddlewareHasCreatedUserActionId_UsesExistingId()
    {
        var userActionId = Guid.NewGuid();

        _httpContext.Items[UserActionIdConstants.HttpContextItemKey] =
            userActionId;

        _currentUser.UserId.Returns(101);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(201);
        _currentUser.IsMat.Returns(false);

        var service = BuildService();

        await service.RecordActionAsync();

        await _userActionRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<UserActionEntity>(entity =>
                    entity.Id == userActionId
                )
            );

        Assert.Equal(
            userActionId,
            _httpContext.Items[UserActionIdConstants.HttpContextItemKey]
        );
    }

    [Fact]
    public async Task RecordAsync_WhenActionAlreadyRecorded_DoesNotCreateAnotherUserAction()
    {
        _httpContext.Items[
            UserActionIdConstants.HttpContextItemKey
        ] = Guid.NewGuid();

        _httpContext.Items[
            UserActionIdConstants.RecordedHttpContextItemKey
        ] = true;

        _currentUser.UserId.Returns(101);

        var service = BuildService();

        await service.RecordActionAsync();

        await _userActionRepository
            .DidNotReceive()
            .CreateAsync(Arg.Any<UserActionEntity>());
    }
}

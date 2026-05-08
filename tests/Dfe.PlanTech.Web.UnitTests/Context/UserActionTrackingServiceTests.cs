using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Context.Interfaces;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Context;

public class UserActionTrackingServiceTests
{
    private readonly IUserActionRepository _userActionRepository = Substitute.For<IUserActionRepository>();
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly DefaultHttpContext _httpContext = new();

    private UserActionTrackingService BuildService()
    {
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        return new UserActionTrackingService(
            _userActionRepository,
            _httpContextAccessor,
            _currentUser
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

        await service.RecordAsync();

        await _userActionRepository.Received(1).CreateAsync(
            Arg.Is<UserActionEntity>(entity =>
                entity.Id != Guid.Empty &&
                entity.UserId == 101 &&
                entity.EstablishmentId == 201 &&
                entity.MatEstablishmentId == null &&
                entity.RequestedUrl == "/test-path?id=1"
            )
        );

        Assert.True(_httpContext.Items.ContainsKey(UserActionIdConstants.HttpContextItemKey));
        Assert.IsType<Guid>(_httpContext.Items[UserActionIdConstants.HttpContextItemKey]);
    }

    [Fact]
    public async Task RecordAsync_WhenCurrentUserIsMat_ThenCreatesUserActionWithMatEstablishmentId()
    {
        _currentUser.UserId.Returns(101);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(201);
        _currentUser.IsMat.Returns(true);
        _currentUser.UserOrganisationId.Returns(301);

        var service = BuildService();

        await service.RecordAsync();

        await _userActionRepository.Received(1).CreateAsync(
            Arg.Is<UserActionEntity>(entity =>
                entity.EstablishmentId == 201 &&
                entity.MatEstablishmentId == 301
            )
        );
    }

    [Fact]
    public async Task RecordAsync_WhenUserIdIsNull_ThenDoesNotCreateUserAction()
    {
        _currentUser.UserId.Returns((int?)null);

        var service = BuildService();

        await service.RecordAsync();

        await _userActionRepository.DidNotReceive().CreateAsync(Arg.Any<UserActionEntity>());
        Assert.False(_httpContext.Items.ContainsKey(UserActionIdConstants.HttpContextItemKey));
    }

    [Fact]
    public async Task RecordAsync_WhenHttpContextIsNull_ThenThrowsInvalidOperationException()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var service = new UserActionTrackingService(
            _userActionRepository,
            _httpContextAccessor,
            _currentUser
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RecordAsync()
        );

        Assert.Equal("No active HttpContext found.", exception.Message);
    }
}

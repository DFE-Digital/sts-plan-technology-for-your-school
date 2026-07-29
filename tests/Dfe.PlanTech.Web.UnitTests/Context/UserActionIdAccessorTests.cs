using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Providers;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Context;

public class UserActionIdAccessorTests
{
    [Fact]
    public void GetUserActionId_WhenHttpContextHasUserActionId_ThenReturnsUserActionId()
    {
        var expectedUserActionId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Items[UserActionIdConstants.HttpContextItemKey] = expectedUserActionId;

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        var accessor = new UserActionIdProvider(httpContextAccessor);

        var result = accessor.GetUserActionId();

        Assert.Equal(expectedUserActionId, result);
    }

    [Fact]
    public void GetUserActionId_WhenHttpContextIsNull_ThenThrowsInvalidOperationException()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var accessor = new UserActionIdProvider(httpContextAccessor);

        var exception = Assert.Throws<InvalidOperationException>(() => accessor.GetUserActionId());

        Assert.Equal("No active HttpContext found.", exception.Message);
    }

    [Fact]
    public void GetUserActionId_WhenUserActionIdIsMissing_ThenThrowsInvalidOperationException()
    {
        var httpContext = new DefaultHttpContext();

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        var accessor = new UserActionIdProvider(httpContextAccessor);

        var exception = Assert.Throws<InvalidOperationException>(() => accessor.GetUserActionId());

        Assert.Equal("User Action Id was not found in the current request.", exception.Message);
    }

    [Fact]
    public void GetUserActionId_WhenUserActionIdIsNotGuid_ThenThrowsInvalidOperationException()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items[UserActionIdConstants.HttpContextItemKey] = "not-a-guid";

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        var accessor = new UserActionIdProvider(httpContextAccessor);

        var exception = Assert.Throws<InvalidOperationException>(() => accessor.GetUserActionId());

        Assert.Equal("User Action Id was not found in the current request.", exception.Message);
    }
}

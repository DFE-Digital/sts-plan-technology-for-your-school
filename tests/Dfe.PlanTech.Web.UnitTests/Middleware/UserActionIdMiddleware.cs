using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class UserActionIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenHeaderContainsValidGuid_ThenUsesHeaderValue()
    {
        var expectedId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers[UserActionIdMiddleware.HeaderName] = expectedId.ToString();

        var middleware = new UserActionIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedId, context.Items[UserActionIdMiddleware.HttpContextItemKey]);
        Assert.Equal(expectedId.ToString(), context.Response.Headers[UserActionIdMiddleware.HeaderName]);
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderMissing_ThenCreatesGuid()
    {
        var context = new DefaultHttpContext();

        var middleware = new UserActionIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var userActionId = Assert.IsType<Guid>(context.Items[UserActionIdMiddleware.HttpContextItemKey]);
        Assert.NotEqual(Guid.Empty, userActionId);
        Assert.Equal(userActionId.ToString(), context.Response.Headers[UserActionIdMiddleware.HeaderName]);
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderInvalid_ThenCreatesGuid()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[UserActionIdMiddleware.HeaderName] = "not-a-guid";

        var middleware = new UserActionIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        var userActionId = Assert.IsType<Guid>(context.Items[UserActionIdMiddleware.HttpContextItemKey]);
        Assert.NotEqual(Guid.Empty, userActionId);
        Assert.NotEqual("not-a-guid", context.Response.Headers[UserActionIdMiddleware.HeaderName].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenCalled_ThenCallsNext()
    {
        var nextWasCalled = false;
        var context = new DefaultHttpContext();

        var middleware = new UserActionIdMiddleware(_ =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(nextWasCalled);
    }
}

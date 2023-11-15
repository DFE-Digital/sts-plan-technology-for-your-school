using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class SecurityHeadersMiddlewareTests
{

    [Fact]
    public async Task Should_Set_Headers()
    {
        var context = new DefaultHttpContext();

        var next = (HttpContext hc) => Task.CompletedTask;

        var middleware = new SecurityHeadersMiddleware(new RequestDelegate(next));

        await middleware.InvokeAsync(context);

        var response = context.Response;

        Assert.NotEmpty(context.Response.Headers.XFrameOptions);
        Assert.Equal("Deny", context.Response.Headers.XFrameOptions);

        Assert.NotEmpty(context.Response.Headers.ContentSecurityPolicy);
        Assert.Equal("frame-ancestors 'none'", context.Response.Headers.ContentSecurityPolicy);
    }
}

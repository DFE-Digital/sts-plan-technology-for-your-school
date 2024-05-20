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

        Assert.True(context.Response.Headers.XFrameOptions.Count > 0);
        Assert.Equal("Deny", context.Response.Headers.XFrameOptions);

        Assert.True(context.Response.Headers.ContentSecurityPolicy.Count > 0);

        var csp = context.Response.Headers.ContentSecurityPolicy.ToString();
        Assert.Contains("frame-ancestors 'none';", csp);
        Assert.Contains("default-src 'self';", csp);
        Assert.Contains("script-src 'nonce-", csp);
    }
}

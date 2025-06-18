using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class SecurityHeadersMiddlewareTests
{

    public readonly IConfiguration Configuration;

    private const string ImgSrcKey = "CSP:ImgSrc";
    private const string ImgSrc = "imgsrc";

    private const string ConnectSrcKey = "CSP:ConnectSrc";
    private const string ConnectSrc = "connectsrc";

    private const string FrameSrcKey = "CSP:FrameSrc";
    private const string FrameSrc = "framesrc";

    private const string ScriptSrcKey = "CSP:ScriptSrc";
    private const string ScriptSrc = "scriptsrc";

    private const string DefaultSrcKey = "CSP:DefaultSrc";
    private const string DefaultSrc = "defaultsrc";

    public SecurityHeadersMiddlewareTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { ImgSrcKey, ImgSrc },
            { ConnectSrcKey, ConnectSrc },
            { FrameSrcKey, FrameSrc },
            { ScriptSrcKey, ScriptSrc },
            { DefaultSrcKey, DefaultSrc }
        };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }


    [Fact]
    public async Task Should_Set_Headers()
    {
        var context = Substitute.For<HttpContext>();
        var config = new CspConfiguration(Configuration);

        context.RequestServices.GetService(typeof(CspConfiguration)).Returns(config);

        var next = (HttpContext hc) => Task.CompletedTask;

        var middleware = new SecurityHeadersMiddleware(new RequestDelegate(next));

        await middleware.InvokeAsync(context);

        Assert.True(context.Response.Headers.XFrameOptions.Count > 0);
        Assert.Equal("Deny", context.Response.Headers.XFrameOptions);

        Assert.True(context.Response.Headers.ContentSecurityPolicy.Count > 0);

        var csp = context.Response.Headers.ContentSecurityPolicy.ToString();
        Assert.Contains("frame-ancestors 'none';", csp);
        Assert.Contains("default-src 'self'", csp);
        Assert.Contains("script-src 'nonce-", csp);
    }
}

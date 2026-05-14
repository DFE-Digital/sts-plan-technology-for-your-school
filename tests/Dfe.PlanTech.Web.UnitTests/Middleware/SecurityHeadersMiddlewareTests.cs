using System.Text.RegularExpressions;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    private const string ImgSrc = "imgsrc";
    private const string ConnectSrc = "connectsrc";
    private const string FrameSrc = "framesrc";
    private const string ScriptSrc = "scriptsrc";
    private const string DefaultSrc = "defaultsrc";

    private static DefaultHttpContext BuildContext()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ContentSecurityPolicy:ImgSrc"] = ImgSrc,
            ["ContentSecurityPolicy:ConnectSrc"] = ConnectSrc,
            ["ContentSecurityPolicy:FrameSrc"] = FrameSrc,
            ["ContentSecurityPolicy:ScriptSrc"] = ScriptSrc,
            ["ContentSecurityPolicy:DefaultSrc"] = DefaultSrc,
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection()
            .Configure<ContentSecurityPolicyConfiguration>(
                configuration.GetRequiredSection("ContentSecurityPolicy")
            )
            .AddSingleton(sp =>
                sp.GetRequiredService<IOptions<ContentSecurityPolicyConfiguration>>().Value
            )
            .BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = services,
            Response = { Body = new MemoryStream() },
        };

        return context;
    }

    [Fact]
    public async Task Should_Set_Headers()
    {
        var context = BuildContext();
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = new SecurityHeadersMiddleware(next);

        await middleware.InvokeAsync(context);

        Assert.True(context.Response.Headers.XFrameOptions.Count > 0);
        Assert.Equal("Deny", context.Response.Headers.XFrameOptions.ToString());

        Assert.True(context.Response.Headers.ContentSecurityPolicy.Count > 0);

        var csp = context.Response.Headers.ContentSecurityPolicy.ToString();

        Assert.Contains("frame-ancestors 'none'", csp);
        Assert.Contains("default-src 'self'", csp);
        Assert.Contains($"default-src 'self' {DefaultSrc}", csp);

        Assert.Contains("script-src 'nonce-", csp);
        Assert.Contains(ScriptSrc, csp);

        Assert.Contains($"img-src 'self' {ImgSrc}", csp);
        Assert.Contains($"connect-src {ConnectSrc}", csp);
        Assert.Contains($"frame-src {FrameSrc}", csp);
    }

    [Fact]
    public async Task Should_Store_Nonce_In_Context_Items()
    {
        var context = BuildContext();
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = new SecurityHeadersMiddleware(next);

        await middleware.InvokeAsync(context);

        var csp = context.Response.Headers.ContentSecurityPolicy.ToString();
        var match = Regex.Match(csp, @"script-src 'nonce-([a-f0-9]{32})'");
        Assert.True(match.Success, $"Expected nonce in CSP header, got: {csp}");

        Assert.True(context.Items.ContainsKey("nonce"));
        Assert.Equal(match.Groups[1].Value, context.Items["nonce"]?.ToString());
    }
}

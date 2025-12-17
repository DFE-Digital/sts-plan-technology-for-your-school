using System.Text.RegularExpressions;
using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Middleware
{
    public class SecurityHeadersMiddlewareTests
    {
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

        private static DefaultHttpContext BuildContext()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { ImgSrcKey, ImgSrc },
                { ConnectSrcKey, ConnectSrc },
                { FrameSrcKey, FrameSrc },
                { ScriptSrcKey, ScriptSrc },
                { DefaultSrcKey, DefaultSrc }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var cfg = new ContentSecurityPolicyConfiguration(configuration);

            var services = new ServiceCollection()
                .AddSingleton(cfg)
                .BuildServiceProvider();

            var ctx = new DefaultHttpContext
            {
                RequestServices = services,
                Response =
                {
                    Body = new MemoryStream()
                }
            };

            return ctx;
        }

        [Fact]
        public async Task Should_Set_Headers()
        {
            var context = BuildContext();
            var hostingEnvironment = Substitute.For<IWebHostEnvironment>();
            var next = new RequestDelegate(_ => Task.CompletedTask);
            var middleware = new SecurityHeadersMiddleware(hostingEnvironment, next);

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
            var hostingEnvironment = Substitute.For<IWebHostEnvironment>();
            var next = new RequestDelegate(_ => Task.CompletedTask);
            var middleware = new SecurityHeadersMiddleware(hostingEnvironment, next);

            await middleware.InvokeAsync(context);

            var csp = context.Response.Headers.ContentSecurityPolicy.ToString();
            var match = Regex.Match(csp, @"script-src 'nonce-([a-f0-9]{32})'");
            Assert.True(match.Success, $"Expected nonce in CSP header, got: {csp}");

            Assert.True(context.Items.ContainsKey("nonce"));
            Assert.Equal(match.Groups[1].Value, context.Items["nonce"]?.ToString());
        }
    }
}

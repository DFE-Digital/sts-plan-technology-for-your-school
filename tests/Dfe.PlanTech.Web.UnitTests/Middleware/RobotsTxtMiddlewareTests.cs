using System.Text;
using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.UnitTests.Middleware
{
    public class RobotsTxtMiddlewareTests
    {
        private static RobotsTxtMiddleware BuildSut(RobotsConfiguration cfg, out Func<bool> wasNextCalled)
        {
            var nextCalled = false;

            RequestDelegate next = _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            wasNextCalled = () => nextCalled;

            var options = Options.Create(cfg);
            return new RobotsTxtMiddleware(next, options);
        }

        private static DefaultHttpContext BuildContext(string path = RobotsTxtMiddlewareExtensions.RobotsTxtPath)
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Path = path;
            ctx.Response.Body = new MemoryStream(); // capture output
            return ctx;
        }

        [Fact]
        public async Task Writes_ContentType_Header_And_Body_From_Config()
        {
            var cfg = new RobotsConfiguration
            {
                UserAgent = "*",
                DisallowedPaths = new[] { "/admin", "/private" },
                CacheMaxAge = 3600
            };

            var sut = BuildSut(cfg, out var _);
            var context = BuildContext();

            await sut.InvokeAsync(context);

            Assert.Equal("text/plain", context.Response.ContentType);
            Assert.Equal("max-age=3600", context.Response.Headers.CacheControl.ToString());

            context.Response.Body.Position = 0;
            var body = Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray());

            body = body.Replace("\r\n", "\n");

            var expected =
                "User-agent: *\n" +
                "Disallow: /admin\n" +
                "Disallow: /private";

            Assert.Equal(expected, body);
        }


        [Fact]
        public async Task Handles_Empty_DisallowedPaths_Cleanly()
        {
            var cfg = new RobotsConfiguration
            {
                UserAgent = "Googlebot",
                DisallowedPaths = new string[0],
                CacheMaxAge = 120
            };

            var sut = BuildSut(cfg, out var _);
            var context = BuildContext();

            await sut.InvokeAsync(context);

            Assert.Equal("text/plain", context.Response.ContentType);
            Assert.Equal("max-age=120", context.Response.Headers.CacheControl.ToString());

            context.Response.Body.Position = 0;
            var body = Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray());

            var expected = "User-agent: Googlebot";
            Assert.Equal(expected, body);
        }

        [Fact]
        public async Task Does_Not_Invoke_Next_Delegate()
        {
            var cfg = new RobotsConfiguration
            {
                UserAgent = "*",
                DisallowedPaths = new[] { "/hidden" },
                CacheMaxAge = 10
            };

            var sut = BuildSut(cfg, out var wasNextCalled);
            var context = BuildContext();

            await sut.InvokeAsync(context);

            Assert.False(wasNextCalled());
        }

        [Fact]
        public void IsRobotsPath_Matches_Only_RobotsTxt()
        {
            var robotsContext = BuildContext("/robots.txt");
            var otherContext = BuildContext("/health");

            var isRobots = RobotsTxtMiddlewareExtensions.IsRobotsPath(robotsContext);
            var isOther = RobotsTxtMiddlewareExtensions.IsRobotsPath(otherContext);

            Assert.True(isRobots);
            Assert.False(isOther);
        }
    }
}

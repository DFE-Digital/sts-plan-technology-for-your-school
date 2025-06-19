using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Web.Configurations;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class RobotsTxtMiddlewareTests
{
    private IOptions<RobotsConfiguration> _options = Substitute.For<IOptions<RobotsConfiguration>>();
    private readonly RobotsTxtMiddleware _middleware;
    private readonly HttpContext _httpContext;

    public RobotsTxtMiddlewareTests()
    {
        _httpContext = Substitute.For<HttpContext>();
        var response = Substitute.For<HttpResponse>();
        response.Body = new MemoryStream();

        _httpContext.Response.Returns(response);

        var next = (HttpContext hc) => Task.CompletedTask;
        _middleware = new RobotsTxtMiddleware(new RequestDelegate(next), _options);
    }

    [Fact]
    public async Task Should_Create_Valid_File()
    {
        var config = new RobotsConfiguration()
        {
            UserAgent = "example-user-agent",
            DisallowedPaths = ["/", "2313", UrlConstants.SelfAssessmentPage],
        };

        _options.Value.Returns(config);
        await _middleware.InvokeAsync(_httpContext);

        var response = await ReadResponseBodyAsync(_httpContext.Response);

        Assert.Contains($"User-agent: {config.UserAgent}", response);

        foreach (var path in config.DisallowedPaths)
        {
            Assert.Contains($"Disallow: {path}", response);
        }

        var cacheControlHeader = _httpContext.Response.Headers.CacheControl;

        Assert.Equal($"max-age={config.CacheMaxAge}", cacheControlHeader);
    }

    protected static Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        response.Body.Position = 0;
        var r = new StreamReader(response.Body);
        return r.ReadToEndAsync();
    }

}

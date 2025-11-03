using System.Net;

namespace Dfe.PlanTech.Web.SiteOfflineMicrosite.UnitTests;

public class StaticAssetsTests : IClassFixture<SiteOfflineMicrositeWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StaticAssetsTests(SiteOfflineMicrositeWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CssFile_WhenRequested_ReturnsHttp200()
    {
        var response = await _client.GetAsync("/css/site.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CssFile_WhenRequested_ReturnsCorrectContentType()
    {
        var response = await _client.GetAsync("/css/site.css");

        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CssFile_WhenRequested_ReturnsNonEmptyContent()
    {
        var response = await _client.GetAsync("/css/site.css");
        var content = await response.Content.ReadAsStringAsync();

        Assert.False(string.IsNullOrWhiteSpace(content));
        Assert.True(content.Length > 0);
    }

    [Fact]
    public async Task JavaScriptFile_WhenRequested_ReturnsHttp200()
    {
        var response = await _client.GetAsync("/js/site.js");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task JavaScriptFile_WhenRequested_ReturnsCorrectContentType()
    {
        var response = await _client.GetAsync("/js/site.js");

        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.True(
            contentType == "application/javascript" ||
            contentType == "text/javascript" ||
            contentType == "application/ecmascript");
    }

    [Fact]
    public async Task NonExistentStaticFile_WhenRequested_ReturnsHttp503()
    {
        var response = await _client.GetAsync("/css/non-existent.css");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task StaticFileInSubdirectory_WhenRequestedWithTraversal_DoesNotExposeFileSystem()
    {
        var response = await _client.GetAsync("/css/../../Program.cs");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/assets/images/favicon.ico")]
    [InlineData("/assets/images/dfe-logo.png")]
    [InlineData("/assets/images/dfe-logo-alt.png")]
    public async Task ImageAssets_WhenRequested_ReturnsHttp200(string imagePath)
    {
        var response = await _client.GetAsync(imagePath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StaticFile_WhenRequestedWithPostMethod_ReturnsHttp503()
    {
        var response = await _client.PostAsync("/css/site.css", null);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task CssFile_WhenRequestedMultipleTimes_ConsistentlyReturnsContent()
    {
        var firstResponse = await _client.GetAsync("/css/site.css");
        var firstContent = await firstResponse.Content.ReadAsStringAsync();

        var secondResponse = await _client.GetAsync("/css/site.css");
        var secondContent = await secondResponse.Content.ReadAsStringAsync();

        Assert.Equal(firstContent, secondContent);
    }

    [Fact]
    public async Task StaticFileWithCacheBusting_WhenRequested_ReturnsHttp200()
    {
        var response = await _client.GetAsync("/css/site.css?v=12345");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}


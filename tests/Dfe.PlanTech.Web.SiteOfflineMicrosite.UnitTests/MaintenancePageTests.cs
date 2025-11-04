using System.Net;

namespace Dfe.PlanTech.Web.SiteOfflineMicrosite.UnitTests;

public class MaintenancePageTests : IClassFixture<SiteOfflineMicrositeWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MaintenancePageTests(SiteOfflineMicrositeWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RootPath_WhenAccessedWithGetRequest_ReturnsHttp503()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task RootPath_WhenAccessedWithGetRequest_ReturnsMaintenancePageContent()
    {
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Sorry, the service is unavailable", content);
        Assert.Contains("temporarily unavailable", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("maintenance", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RootPath_WhenAccessedWithGetRequest_IncludesRetryAfterHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("Retry-After"));
        Assert.Equal("3600", response.Headers.GetValues("Retry-After").First());
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/home")]
    [InlineData("/self-assessment")]
    [InlineData("/questions/submit")]
    [InlineData("/api/data")]
    [InlineData("/admin")]
    [InlineData("/content/pages/123")]
    public async Task AnyPath_WhenAccessedWithGetRequest_ReturnsHttp503(string path)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/questions/submit")]
    [InlineData("/api/data")]
    public async Task AnyPath_WhenAccessedWithPostRequest_ReturnsHttp503(string path)
    {
        var response = await _client.PostAsync(path, null);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/data")]
    [InlineData("/content/pages/123")]
    public async Task AnyPath_WhenAccessedWithPutRequest_ReturnsHttp503(string path)
    {
        var response = await _client.PutAsync(path, null);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/data")]
    [InlineData("/content/pages/123")]
    public async Task AnyPath_WhenAccessedWithDeleteRequest_ReturnsHttp503(string path)
    {
        var response = await _client.DeleteAsync(path);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/data")]
    [InlineData("/content/pages/123")]
    public async Task AnyPath_WhenAccessedWithPatchRequest_ReturnsHttp503(string path)
    {
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), path);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task RootPath_WhenAccessedWithHeadRequest_ReturnsHttp503()
    {
        var request = new HttpRequestMessage(HttpMethod.Head, "/");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task RootPath_WhenAccessedWithOptionsRequest_ReturnsHttp503()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Theory]
    [InlineData("/../admin")]
    [InlineData("/./hidden")]
    [InlineData("/path/../other")]
    [InlineData("/path/./current")]
    public async Task PathTraversal_WhenAttempted_ReturnsHttp503(string maliciousPath)
    {
        var response = await _client.GetAsync(maliciousPath);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Theory]
    [InlineData("/path?query=value")]
    [InlineData("/?redirect=http://evil.com")]
    [InlineData("/admin?bypass=true")]
    public async Task PathsWithQueryStrings_WhenAccessed_ReturnsHttp503(string pathWithQuery)
    {
        var response = await _client.GetAsync(pathWithQuery);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task VeryLongPath_WhenAccessed_ReturnsHttp503()
    {
        var longPath = "/" + new string('a', 2000);
        var response = await _client.GetAsync(longPath);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Theory]
    [InlineData("/path%20with%20spaces")]
    [InlineData("/path%2F%2F%2Fslashes")]
    [InlineData("/%3Cscript%3Ealert('xss')%3C/script%3E")]
    public async Task EncodedPaths_WhenAccessed_ReturnsHttp503(string encodedPath)
    {
        var response = await _client.GetAsync(encodedPath);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task MultipleRequests_WhenMadeSequentially_AllReturnHttp503()
    {
        var paths = new[] { "/", "/home", "/api", "/admin", "/questions" };

        foreach (var path in paths)
        {
            var response = await _client.GetAsync(path);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
    }

    [Theory]
    [InlineData("text/html")]
    [InlineData("application/json")]
    [InlineData("application/xml")]
    [InlineData("*/*")]
    public async Task RootPath_WhenAccessedWithDifferentAcceptHeaders_ReturnsHttp503(string acceptHeader)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Accept", acceptHeader);

        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task RootPath_WhenAccessedWithCustomUserAgent_ReturnsHttp503()
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("User-Agent", "MaliciousBot/1.0");

        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task RootPath_WhenAccessedWithLargeHeaders_ReturnsHttp503()
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Custom-Header", new string('x', 1000));

        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task RootPath_WhenAccessedWithPostAndLargeBody_ReturnsHttp503()
    {
        var largeContent = new StringContent(new string('x', 10000));
        var response = await _client.PostAsync("/", largeContent);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task DeepNestedPath_WhenAccessed_ReturnsHttp503()
    {
        var deepPath = "/" + string.Join("/", Enumerable.Repeat("level", 50));
        var response = await _client.GetAsync(deepPath);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Theory]
    [InlineData("/null")]
    [InlineData("/undefined")]
    [InlineData("/NaN")]
    public async Task PathsWithSpecialValues_WhenAccessed_ReturnsHttp503(string specialPath)
    {
        var response = await _client.GetAsync(specialPath);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}


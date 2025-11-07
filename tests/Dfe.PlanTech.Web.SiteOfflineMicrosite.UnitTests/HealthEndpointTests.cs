using System.Net;
using System.Text.Json;

namespace Dfe.PlanTech.Web.SiteOfflineMicrosite.UnitTests;

public class HealthEndpointTests : IClassFixture<SiteOfflineMicrositeWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(SiteOfflineMicrositeWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_WhenAccessed_ReturnsHttp200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_WhenAccessed_ReturnsJsonResponse()
    {
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        var json = JsonDocument.Parse(content);
        Assert.NotNull(json);
    }

    [Fact]
    public async Task HealthEndpoint_WhenAccessed_IndicatesMaintenanceStatus()
    {
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        Assert.Equal("maintenance", json.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task HealthEndpoint_WhenAccessed_IndicatesMicrositeIsHealthy()
    {
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        Assert.True(json.RootElement.GetProperty("healthy").GetBoolean());
    }

    [Fact]
    public async Task HealthEndpoint_WhenAccessed_IndicatesMainSiteUnavailable()
    {
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        Assert.False(json.RootElement.GetProperty("mainSiteAvailable").GetBoolean());
    }

    [Fact]
    public async Task HealthEndpoint_WhenAccessed_IncludesTimestamp()
    {
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        var timestamp = json.RootElement.GetProperty("timestamp").GetDateTime();
        var timeDifference = Math.Abs((DateTime.UtcNow - timestamp).TotalMinutes);
        Assert.True(timeDifference < 1);
    }

    [Fact]
    public async Task HealthEndpoint_WhenAccessed_IncludesDescriptiveMessage()
    {
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        var message = json.RootElement.GetProperty("message").GetString();
        Assert.NotNull(message);
        Assert.False(string.IsNullOrWhiteSpace(message));
        Assert.True(message.Contains("maintenance", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task HealthEndpoint_WhenAccessedMultipleTimes_ConsistentlyReturnsHttp200()
    {
        for (int i = 0; i < 5; i++)
        {
            var response = await _client.GetAsync("/health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task HealthEndpoint_WhenAccessedWithPostRequest_ReturnsMaintenance503()
    {
        var response = await _client.PostAsync("/health", null);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_DoesNotIncludeRetryAfterHeader()
    {
        var response = await _client.GetAsync("/health");

        Assert.False(response.Headers.Contains("Retry-After"));
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("*/*")]
    [InlineData("text/html")]
    public async Task HealthEndpoint_WhenAccessedWithDifferentAcceptHeaders_ReturnsHttp200(string acceptHeader)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Accept", acceptHeader);

        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_WhenAccessedFromDifferentOrigin_ReturnsHttp200()
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Origin", "https://external-monitor.example.com");

        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_WhenAccessedWithHeadRequest_ReturnsHttp200()
    {
        var request = new HttpRequestMessage(HttpMethod.Head, "/health");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}


namespace Dfe.PlanTech.Web.SiteOfflineMicrosite.UnitTests;

public class SecurityHeadersTests : IClassFixture<SiteOfflineMicrositeWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SecurityHeadersTests(SiteOfflineMicrositeWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_DoesNotExposeServerHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.False(response.Headers.Contains("Server"));
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_DoesNotExposeXPoweredByHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.False(response.Headers.Contains("X-Powered-By"));
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_DoesNotExposeXAspNetVersionHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.False(response.Headers.Contains("X-AspNet-Version"));
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_DoesNotExposeXAspNetMvcVersionHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.False(response.Headers.Contains("X-AspNetMvc-Version"));
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_IncludesXContentTypeOptionsHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_IncludesXFrameOptionsHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.Equal("deny", response.Headers.GetValues("X-Frame-Options").First());
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_IncludesContentSecurityPolicyHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        var csp = response.Headers.GetValues("Content-Security-Policy").First();
        Assert.Contains("default-src 'self'", csp);
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_IncludesStrictTransportSecurityHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("Strict-Transport-Security"));
        var hsts = response.Headers.GetValues("Strict-Transport-Security").First();
        Assert.Contains("max-age=", hsts);
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_IncludesReferrerPolicyHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("Referrer-Policy"));
        var referrerPolicy = response.Headers.GetValues("Referrer-Policy").First();
        Assert.True(
            referrerPolicy == "no-referrer" ||
            referrerPolicy == "strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_IncludesPermissionsPolicyHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("Permissions-Policy"));
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_IncludesXDnsPrefetchControlHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("X-DNS-Prefetch-Control"));
        Assert.Equal("off", response.Headers.GetValues("X-DNS-Prefetch-Control").First());
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_IncludesXPermittedCrossDomainPoliciesHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("X-Permitted-Cross-Domain-Policies"));
        Assert.Equal("none", response.Headers.GetValues("X-Permitted-Cross-Domain-Policies").First());
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_IncludesCrossOriginEmbedderPolicyHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("Cross-Origin-Embedder-Policy"));
        Assert.Equal("require-corp", response.Headers.GetValues("Cross-Origin-Embedder-Policy").First());
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_IncludesCrossOriginOpenerPolicyHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("Cross-Origin-Opener-Policy"));
        Assert.Equal("same-origin", response.Headers.GetValues("Cross-Origin-Opener-Policy").First());
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_IncludesCrossOriginResourcePolicyHeader()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("Cross-Origin-Resource-Policy"));
        Assert.Equal("same-origin", response.Headers.GetValues("Cross-Origin-Resource-Policy").First());
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_ReferrerPolicyIsNoReferrer()
    {
        var response = await _client.GetAsync("/");

        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").First());
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_CacheControlMatchesOwaspRecommendation()
    {
        var response = await _client.GetAsync("/");

        var cacheControl = response.Headers.CacheControl;
        Assert.NotNull(cacheControl);
        Assert.True(cacheControl.NoStore);
        Assert.Equal(0, cacheControl.MaxAge?.TotalSeconds);
    }

    [Fact]
    public async Task HealthEndpoint_WhenRequested_DoesNotExposeServerHeader()
    {
        var response = await _client.GetAsync("/health");

        Assert.False(response.Headers.Contains("Server"));
    }

    [Fact]
    public async Task HealthEndpoint_WhenRequested_DoesNotExposeXPoweredByHeader()
    {
        var response = await _client.GetAsync("/health");

        Assert.False(response.Headers.Contains("X-Powered-By"));
    }

    [Fact]
    public async Task StaticAsset_WhenRequested_DoesNotExposeServerHeader()
    {
        var response = await _client.GetAsync("/css/site.css");

        Assert.False(response.Headers.Contains("Server"));
    }

    [Fact]
    public async Task MaintenancePage_WhenRequestedViaPost_DoesNotExposeServerHeader()
    {
        var response = await _client.PostAsync("/", null);

        Assert.False(response.Headers.Contains("Server"));
    }

    [Fact]
    public async Task MaintenancePage_WhenRequestedMultipleTimes_ConsistentlyRemovesServerHeaders()
    {
        for (int i = 0; i < 3; i++)
        {
            var response = await _client.GetAsync("/");
            Assert.False(response.Headers.Contains("Server"));
            Assert.False(response.Headers.Contains("X-Powered-By"));
        }
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/admin")]
    [InlineData("/api/data")]
    public async Task VariousPaths_WhenRequested_DoNotExposeVersionInformation(string path)
    {
        var response = await _client.GetAsync(path);

        Assert.False(response.Headers.Contains("X-AspNet-Version"));
        Assert.False(response.Headers.Contains("X-AspNetMvc-Version"));
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_DoesNotLeakApplicationDetails()
    {
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("ASP.NET", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Kestrel", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".NET", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ErrorResponse_WhenTriggered_DoesNotExposeServerHeader()
    {
        var response = await _client.GetAsync("/this-will-cause-error-if-misconfigured");

        Assert.False(response.Headers.Contains("Server"));
    }

    [Fact]
    public async Task MaintenancePage_WhenRequested_CacheControlHeaderPreventsCache()
    {
        var response = await _client.GetAsync("/");

        var cacheControl = response.Headers.CacheControl;
        Assert.True(cacheControl?.NoCache == true || cacheControl?.NoStore == true);
    }

    [Fact]
    public async Task HealthEndpoint_WhenRequested_AllowsReasonableCaching()
    {
        var response = await _client.GetAsync("/health");

        var cacheControl = response.Headers.CacheControl;

        if (cacheControl?.MaxAge.HasValue == true)
        {
            Assert.True(cacheControl.MaxAge.Value.TotalSeconds <= 60);
        }
    }
}


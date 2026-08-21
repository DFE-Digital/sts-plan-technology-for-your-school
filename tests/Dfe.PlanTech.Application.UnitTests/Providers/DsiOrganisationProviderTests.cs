using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using Dfe.PlanTech.Application.Providers;
using Dfe.PlanTech.Core.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Application.UnitTests.Providers;

public class DsiOrganisationProviderTests
{
    private const string ClientId = "test-client-id";

    // HS256 requires a key of at least 128 bits (16 bytes) - use a sufficiently long secret
    // to avoid a spurious ArgumentOutOfRangeException from SymmetricSecurityKey.
    private const string TestValue = "this-is-a-sufficiently-long-test-secret-key-1234567890";

    private const string BaseAddress = "https://dsi.example.local/";

    private static DfeSignInConfiguration CreateConfig() =>
        new() { ClientId = ClientId, ApiSecret = TestValue };

    private static (DsiOrganisationProvider sut, FakeHttpMessageHandler handler) CreateSut(
        Func<HttpRequestMessage, HttpResponseMessage> responder
    )
    {
        var handler = new FakeHttpMessageHandler(responder);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) };

        var sut = new DsiOrganisationProvider(CreateConfig(), httpClient);
        return (sut, handler);
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) =>
        new(statusCode) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task GetOrganisationForUserAsync_ReturnsMatchingEstablishment_WhenUrnMatches()
    {
        // Arrange
        const string json = """
            [
                { "urn": "111111" },
                { "urn": "222222" }
            ]
            """;

        var (sut, _) = CreateSut(_ => JsonResponse(HttpStatusCode.OK, json));

        // Act
        var result = await sut.GetOrganisationForUserAsync("user-ref", "222222");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("222222", result!.Urn);
    }

    [Fact]
    public async Task GetOrganisationForUserAsync_ReturnsNull_WhenNoMatchingUrn()
    {
        const string json = """[ { "Urn": "111111" } ]""";

        var (sut, _) = CreateSut(_ => JsonResponse(HttpStatusCode.OK, json));

        var result = await sut.GetOrganisationForUserAsync("user-ref", "does-not-exist");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrganisationForUserAsync_ReturnsNull_WhenApiReturnsEmptyArray()
    {
        var (sut, _) = CreateSut(_ => JsonResponse(HttpStatusCode.OK, "[]"));

        var result = await sut.GetOrganisationForUserAsync("user-ref", "222222");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrganisationForUserAsync_ReturnsNull_WhenApiReturnsJsonNull()
    {
        // JsonSerializer.Deserialize returns null for a "null" payload; the class falls back to [].
        var (sut, _) = CreateSut(_ => JsonResponse(HttpStatusCode.OK, "null"));

        var result = await sut.GetOrganisationForUserAsync("user-ref", "222222");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrganisationForUserAsync_ComparisonIsCaseSensitive()
    {
        // string.Equals(string, string) is an ordinal comparison, so casing must match exactly.
        const string json = """[ { "Urn": "abc123" } ]""";

        var (sut, _) = CreateSut(_ => JsonResponse(HttpStatusCode.OK, json));

        var result = await sut.GetOrganisationForUserAsync("user-ref", "ABC123");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrganisationForUserAsync_Throws_WhenMultipleOrganisationsMatchUrn()
    {
        // SingleOrDefault throws if more than one element matches the predicate.
        const string json = """
            [
                { "urn": "222222" },
                { "urn": "222222" }
            ]
            """;

        var (sut, _) = CreateSut(_ => JsonResponse(HttpStatusCode.OK, json));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GetOrganisationForUserAsync("user-ref", "222222")
        );
    }

    [Fact]
    public async Task GetOrganisationForUserAsync_Throws_WhenApiReturnsNonSuccessStatusCode()
    {
        var (sut, _) = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.GetOrganisationForUserAsync("user-ref", "222222")
        );
    }

    [Fact]
    public async Task GetOrganisationForUserAsync_RequestsCorrectEndpoint_UsingGet()
    {
        var (sut, handler) = CreateSut(_ => JsonResponse(HttpStatusCode.OK, "[]"));

        await sut.GetOrganisationForUserAsync("user-abc-123", "222222");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Equal(
            new Uri(BaseAddress + "users/user-abc-123/organisations"),
            handler.LastRequest.RequestUri
        );
    }

    [Fact]
    public async Task GetOrganisationForUserAsync_SendsBearerToken_SignedWithConfiguredSecret()
    {
        var (sut, handler) = CreateSut(_ => JsonResponse(HttpStatusCode.OK, "[]"));

        await sut.GetOrganisationForUserAsync("user-ref", "222222");

        var authHeader = handler.LastRequest!.Headers.Authorization;
        Assert.NotNull(authHeader);
        Assert.Equal("Bearer", authHeader!.Scheme);
        Assert.False(string.IsNullOrWhiteSpace(authHeader.Parameter));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestValue));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = ClientId,
            ValidateAudience = true,
            ValidAudience = "signin.education.gov.uk",
            ValidateLifetime = true,
            IssuerSigningKey = securityKey,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        };

        var jwtHandler = new JwtSecurityTokenHandler();

        // Throws if the signature, issuer, audience, or expiry don't check out.
        var principal = jwtHandler.ValidateToken(
            authHeader.Parameter,
            validationParameters,
            out var validatedToken
        );

        Assert.NotNull(principal);
        var jwt = Assert.IsType<JwtSecurityToken>(validatedToken);
        Assert.Equal(SecurityAlgorithms.HmacSha256, jwt.SignatureAlgorithm);
    }

    [Fact]
    public async Task GetOrganisationForUserAsync_GeneratesTokenExpiringInApproximatelyFiveMinutes()
    {
        var (sut, handler) = CreateSut(_ => JsonResponse(HttpStatusCode.OK, "[]"));

        var before = DateTime.UtcNow;
        await sut.GetOrganisationForUserAsync("user-ref", "222222");
        var after = DateTime.UtcNow;

        var token = handler.LastRequest!.Headers.Authorization!.Parameter;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.True(jwt.ValidTo >= before.AddMinutes(5).AddSeconds(-5));
        Assert.True(jwt.ValidTo <= after.AddMinutes(5).AddSeconds(5));
    }

    [Fact]
    public async Task GetOrganisationForUserAsync_GeneratesFreshTokenOnEachCall()
    {
        var (sut, handler) = CreateSut(_ => JsonResponse(HttpStatusCode.OK, "[]"));

        await sut.GetOrganisationForUserAsync("user-ref", "222222");
        var firstToken = handler.LastRequest!.Headers.Authorization!.Parameter;

        // exp has one-second resolution, so wait for it to roll over between calls.
        await Task.Delay(1100, CancellationToken.None);

        await sut.GetOrganisationForUserAsync("user-ref", "222222");
        var secondToken = handler.LastRequest!.Headers.Authorization!.Parameter;

        Assert.NotEqual(firstToken, secondToken);
    }

    /// <summary>
    /// Minimal HttpMessageHandler test double. HttpMessageHandler.SendAsync is protected,
    /// so NSubstitute - which can only substitute accessible members - can't mock it directly.
    /// This fake is the standard workaround for exercising HttpClient-based code in tests.
    /// </summary>
    private sealed class FakeHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder
    ) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            LastRequest = request;
            return Task.FromResult(responder(request));
        }
    }
}

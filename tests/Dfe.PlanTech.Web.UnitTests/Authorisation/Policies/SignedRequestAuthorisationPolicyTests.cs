using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Web.Authorisation.Policies;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Authorisation.Policies;

public class SignedRequestAuthorisationPolicyTests
{
    private readonly ILogger<SignedRequestAuthorisationPolicy> _logger = Substitute.For<ILogger<SignedRequestAuthorisationPolicy>>();

    private const string MockSigningSecret = "super-secret-signing-secret";
    private const string MockHeaderSignedValues = "x-contentful-signed-headers,x-contentful-timestamp";
    private const string MockRequestBody = "{\"body\":\"something\"}";
    private const string CorrectSignature = "6af1bb2c169c7e4ef43486f6c6f5e0ea676bda94eab980c3eb39a52c016b1808";

    private static AuthorizationHandlerContext BuildHandlerContext(HttpContext resource)
    {
        var requirement = new SignedRequestAuthorisationRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // auth status irrelevant
        return new AuthorizationHandlerContext([requirement], user, resource);
    }

    private static HttpContext BuildHttpContext(
        string method = "POST",
        string path = "/webhook",
        string? query = null,
        string? body = null,
        string? secret = null,
        int requestTimeDiff = -1
    )
    {
        var requestTime = DateTime.UtcNow.AddMinutes(requestTimeDiff);
        var requestTimestamp = new DateTimeOffset(requestTime).ToUnixTimeMilliseconds().ToString();
        //var signature = SignedRequestAuthorisationPolicy.CreateCanonicalRepresentation(request, MockHeaderSignedValues);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.test");
        httpContext.Request.Path = new PathString(path);
        httpContext.Request.QueryString = string.IsNullOrEmpty(query) ? QueryString.Empty : new QueryString(query);

        httpContext.Request.Headers.Append(SignedRequestAuthorisationPolicy.HeaderSignedValues, MockHeaderSignedValues);
        httpContext.Request.Headers.Append(SignedRequestAuthorisationPolicy.HeaderSignature, CorrectSignature);
        httpContext.Request.Headers.Append(SignedRequestAuthorisationPolicy.HeaderTimestamp, requestTimestamp);
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(MockRequestBody));
        httpContext.Request.Path = new PathString("/api/cms/webhook");

        if (body != null)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            httpContext.Request.Body = new MemoryStream(bytes);
        }

        return httpContext;
    }

    private SignedRequestAuthorisationPolicy BuildPolicy(string? secret = null)
    {
        var config = new SigningSecretConfiguration() { SigningSecret = secret ?? MockSigningSecret };
        return new SignedRequestAuthorisationPolicy(_logger, config);

    }

    private static string Hmac(string message, string key)
    {
        var mac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = mac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    [Fact]
    public async Task HandleRequirementAsync_When_Secret_Missing_Fails_And_Logs()
    {
        var httpContext = BuildHttpContext();
        var handlerContext = CreateHandlerContext(httpContext);
        var authorisationPolicy = BuildPolicy(secret: string.Empty);

        var ctx = BuildHandlerContext(BuildHttpContext());

        await authorisationPolicy.HandleAsync(ctx);

        Assert.True(ctx.HasFailed);
        _logger.ReceivedWithAnyArgs(1).Log(default, default, default!, default, default!);
    }

    [Fact]
    public async Task HandleRequirementAsync_When_Missing_Headers_Fails_And_Logs()
    {
        var http = BuildHttpContext();
        // no headers set
        var ctx = BuildHandlerContext(http);

        var authorisationPolicy = BuildPolicy(secret: string.Empty);

        await authorisationPolicy.HandleAsync(ctx);

        Assert.True(ctx.HasFailed);
        _logger.ReceivedWithAnyArgs().Log(
            LogLevel.Error, default, Arg.Any<object>(), null, Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task HandleRequirementAsync_When_Timestamp_Expired_Fails_And_Logs()
    {
        var http = BuildHttpContext(body: "{\"x\":1}");
        http.Request.Headers["content-type"] = "application/json";

        // Signed headers list (must correspond to actual headers you want included)
        http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderSignedValues] = "content-type";

        // Expired timestamp
        var past = DateTimeOffset.UtcNow.AddMinutes(-(SignedRequestAuthorisationPolicy.RequestTimeToLiveMinutes + 1))
                                        .ToUnixTimeMilliseconds().ToString();
        http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderTimestamp] = past;

        // Build message and bogus signature (won’t matter—timestamp already expired)
        var canonical = await SignedRequestAuthorisationPolicy.CreateCanonicalRepresentation(
            http.Request,
            http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderSignedValues]);
        http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderSignature] = Hmac(canonical, MockSigningSecret);

        var ctx = BuildHandlerContext(http);
        var authorisationPolicy = BuildPolicy(secret: string.Empty);

        await authorisationPolicy.HandleAsync(ctx);

        Assert.True(ctx.HasFailed);
        _logger.ReceivedWithAnyArgs().Log(
            LogLevel.Error, default, Arg.Any<object>(), null, Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task HandleRequirementAsync_When_Signature_Valid_Succeeds()
    {
        var http = BuildHttpContext(method: "POST", path: "/webhook", query: "?a=1&b=2", body: "{\"id\":\"123\"}");

        // headers included in canonical string
        http.Request.Headers["content-type"] = "application/json";
        http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderSignedValues] = MockHeaderSignedValues;

        // fresh timestamp
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderTimestamp] = nowMs;

        // compute canonical representation and valid signature
        var canonical = await SignedRequestAuthorisationPolicy.CreateCanonicalRepresentation(
            http.Request,
            http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderSignedValues]);
        http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderSignature] = Hmac(canonical, MockSigningSecret);

        var ctx = BuildHandlerContext(http);
        var authorisationPolicy = BuildPolicy();

        await authorisationPolicy.HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
        Assert.False(ctx.HasFailed);
        _logger.DidNotReceiveWithAnyArgs().Log(LogLevel.Error, default, default!, default, default!);
    }

    [Fact]
    public async Task HandleRequirementAsync_When_Resource_Not_HttpContext_Fails_And_Logs()
    {
        var requirement = new SignedRequestAuthorisationRequirement();
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var ctx = new AuthorizationHandlerContext([requirement], principal, resource: new object());
        var authorisationPolicy = BuildPolicy(secret: string.Empty);

        await authorisationPolicy.HandleAsync(ctx);

        Assert.True(ctx.HasFailed);
        _logger.ReceivedWithAnyArgs().Log(LogLevel.Error, default, default!, default, default!);
    }

    [Fact]
    public async Task CreateCanonicalRepresentation_Produces_Expected_Format()
    {
        var http = BuildHttpContext(method: "PUT", path: "/some/path", query: "?q=1", body: "BODY");
        http.Request.Headers["content-type"] = "application/json";
        http.Request.Headers["x-extra"] = "abc";
        var signedHeaders = new StringValues("content-type,x-extra");

        var canonical = await SignedRequestAuthorisationPolicy.CreateCanonicalRepresentation(http.Request, signedHeaders);

        var expectedPathAndQuery = http.Request.GetEncodedPathAndQuery();
        // header order is as provided in signedHeaders, lower-cased, joined with ';'
        var expectedHeaders = "content-type:application/json;x-extra:abc";
        var expected = string.Join("\n", "PUT", expectedPathAndQuery, expectedHeaders, "BODY");

        Assert.Equal(expected, canonical);
    }

    [Fact]
    public async Task Should_Generate_Correct_Canonical_Representation()
    {
        const string canonicalRepresentation = "POST\n/api/cms/webhook\nx-contentful-signed-headers:x-contentful-signed-headers,x-contentful-timestamp;x-contentful-timestamp:1704103200000\n{\"body\":\"something\"}";

        var requestTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var requestTimestamp = new DateTimeOffset(requestTime).ToUnixTimeMilliseconds().ToString();
        var httpContext = BuildHttpContext();
        httpContext.Request.Headers.Remove(SignedRequestAuthorisationPolicy.HeaderTimestamp);
        httpContext.Request.Headers.Append(SignedRequestAuthorisationPolicy.HeaderTimestamp, requestTimestamp);

        var representation = await SignedRequestAuthorisationPolicy.CreateCanonicalRepresentation(httpContext.Request, MockHeaderSignedValues);
        Assert.Equal(canonicalRepresentation, representation);
    }

    [Fact]
    public async Task ShouldReturn_Unauthorised_Result_If_Missing_Headers()
    {
        var httpContext = new DefaultHttpContext();
        var handlerContext = CreateHandlerContext(httpContext);
        var authorisationPolicy = BuildPolicy(secret: string.Empty);

        await authorisationPolicy.HandleAsync(handlerContext);
        Assert.True(handlerContext.HasFailed);
    }

    [Fact]
    public async Task ShouldReturn_Unauthorised_Result_If_Expired_Timestamp()
    {
        var httpContext = BuildHttpContext(requestTimeDiff: 1);
        var handlerContext = CreateHandlerContext(httpContext);
        var authorisationPolicy = BuildPolicy();

        await authorisationPolicy.HandleAsync(handlerContext);
        Assert.True(handlerContext.HasFailed);
    }

    [Fact]
    public async Task ShouldReturn_Unauthorised_Result_If_Incorrect_SigningSecret()
    {
        var config = new SigningSecretConfiguration() { SigningSecret = "incorrect-secret" };
        var authHandler = new SignedRequestAuthorisationPolicy(_logger, config);

        var httpContext = BuildHttpContext();
        var handlerContext = CreateHandlerContext(httpContext);
        await authHandler.HandleAsync(handlerContext);
        Assert.True(handlerContext.HasFailed);
    }

    private static AuthorizationHandlerContext CreateHandlerContext(HttpContext httpContext)
    {
        IEnumerable<IAuthorizationRequirement> requirements = [new SignedRequestAuthorisationRequirement()];
        var user = httpContext.User;
        return new AuthorizationHandlerContext(requirements, user, httpContext);
    }
}

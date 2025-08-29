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
    private static SignedRequestAuthorisationPolicy SUT(string? secret, out ILogger<SignedRequestAuthorisationPolicy> logger)
    {
        logger = Substitute.For<ILogger<SignedRequestAuthorisationPolicy>>();
        var cfg = new SigningSecretConfiguration { SigningSecret = secret };
        return new SignedRequestAuthorisationPolicy(logger, cfg);
    }

    private static AuthorizationHandlerContext AuthzCtx(HttpContext resource)
    {
        var requirement = new SignedRequestAuthorisationRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // auth status irrelevant
        return new AuthorizationHandlerContext([requirement], user, resource);
    }

    private static HttpContext NewHttp(string method = "POST", string path = "/webhook", string? query = null, string? body = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = method;
        ctx.Request.Scheme = "https";
        ctx.Request.Host = new HostString("example.test");
        ctx.Request.Path = new PathString(path);
        ctx.Request.QueryString = string.IsNullOrEmpty(query) ? QueryString.Empty : new QueryString(query);

        if (body != null)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            ctx.Request.Body = new MemoryStream(bytes);
        }

        return ctx;
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
        var sut = SUT(secret: null, out var logger);
        var ctx = AuthzCtx(NewHttp());

        await sut.HandleAsync(ctx);

        Assert.True(ctx.HasFailed);
        logger.ReceivedWithAnyArgs(1).Log(default, default, default!, default, default!);
    }

    [Fact]
    public async Task HandleRequirementAsync_When_Missing_Headers_Fails_And_Logs()
    {
        var sut = SUT("secret", out var logger);
        var http = NewHttp();
        // no headers set
        var ctx = AuthzCtx(http);

        await sut.HandleAsync(ctx);

        Assert.True(ctx.HasFailed);
        logger.ReceivedWithAnyArgs().Log(
            LogLevel.Error, default, Arg.Any<object>(), null, Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task HandleRequirementAsync_When_Timestamp_Expired_Fails_And_Logs()
    {
        var secret = "super-secret";
        var sut = SUT(secret, out var logger);
        var http = NewHttp(body: "{\"x\":1}");
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
        http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderSignature] = Hmac(canonical, secret);

        var ctx = AuthzCtx(http);
        await sut.HandleAsync(ctx);

        Assert.True(ctx.HasFailed);
        logger.ReceivedWithAnyArgs().Log(
            LogLevel.Error, default, Arg.Any<object>(), null, Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task HandleRequirementAsync_When_Signature_Valid_Succeeds()
    {
        var secret = "super-secret";
        var sut = SUT(secret, out var logger);
        var http = NewHttp(method: "POST", path: "/webhook", query: "?a=1&b=2", body: "{\"id\":\"123\"}");

        // headers included in canonical string
        http.Request.Headers["content-type"] = "application/json";
        http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderSignedValues] = "content-type";

        // fresh timestamp
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderTimestamp] = nowMs;

        // compute canonical representation and valid signature
        var canonical = await SignedRequestAuthorisationPolicy.CreateCanonicalRepresentation(
            http.Request,
            http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderSignedValues]);
        http.Request.Headers[SignedRequestAuthorisationPolicy.HeaderSignature] = Hmac(canonical, secret);

        var ctx = AuthzCtx(http);
        await sut.HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
        Assert.False(ctx.HasFailed);
        logger.DidNotReceiveWithAnyArgs().Log(LogLevel.Error, default, default!, default, default!);
    }

    [Fact]
    public async Task HandleRequirementAsync_When_Resource_Not_HttpContext_Fails_And_Logs()
    {
        var sut = SUT("secret", out var logger);
        var requirement = new SignedRequestAuthorisationRequirement();
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var ctx = new AuthorizationHandlerContext([requirement], principal, resource: new object());

        await sut.HandleAsync(ctx);

        Assert.True(ctx.HasFailed);
        logger.ReceivedWithAnyArgs().Log(LogLevel.Error, default, default!, default, default!);
    }

    [Fact]
    public async Task CreateCanonicalRepresentation_Produces_Expected_Format()
    {
        var http = NewHttp(method: "PUT", path: "/some/path", query: "?q=1", body: "BODY");
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
}

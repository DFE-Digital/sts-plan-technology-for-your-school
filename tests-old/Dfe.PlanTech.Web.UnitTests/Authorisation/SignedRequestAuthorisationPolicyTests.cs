using System.Text;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Web.Authorisation.Policies;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Authorisation;

public class SignedRequestAuthorisationPolicyTests
{
    private const string MockSigningSecret = "super-secret-signing-secret";
    private const string MockHeaderSignedValues =
        "x-contentful-signed-headers,x-contentful-timestamp";
    private const string MockRequestBody = "{\"body\":\"something\"}";
    private const string CorrectSignature =
        "6af1bb2c169c7e4ef43486f6c6f5e0ea676bda94eab980c3eb39a52c016b1808";

    private readonly DateTime _requestTime;
    private readonly string _requestTimestamp;

    private readonly SignedRequestAuthorisationPolicy _authorisationPolicy;
    private readonly ILogger<SignedRequestAuthorisationPolicy> _logger = Substitute.For<
        ILogger<SignedRequestAuthorisationPolicy>
    >();
    private readonly ISystemTime _systemTime = Substitute.For<ISystemTime>();

    public SignedRequestAuthorisationPolicyTests()
    {
        var config = new SigningSecretConfiguration() { SigningSecret = MockSigningSecret };
        _authorisationPolicy = new SignedRequestAuthorisationPolicy(_systemTime, config, _logger);

        _requestTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        _requestTimestamp = new DateTimeOffset(_requestTime).ToUnixTimeMilliseconds().ToString();
        _systemTime.UtcNow.Returns(_ => _requestTime);
    }

    [Fact]
    public async Task Should_Generate_Correct_Canonical_Representation()
    {
        const string canonicalRepresentation =
            "POST\n/api/cms/webhook\nx-contentful-signed-headers:x-contentful-signed-headers,x-contentful-timestamp;x-contentful-timestamp:1704103200000\n{\"body\":\"something\"}";
        var httpContext = CreateMockHttpContext();
        var representation = await SignedRequestAuthorisationPolicy.CreateCanonicalRepresentation(
            httpContext.Request,
            MockHeaderSignedValues
        );
        Assert.Equal(canonicalRepresentation, representation);
    }

    [Fact]
    public async Task ShouldReturn_Unauthorised_Result_If_Missing_Headers()
    {
        var httpContext = new DefaultHttpContext();
        var handlerContext = CreateHandlerContext(httpContext);
        await _authorisationPolicy.HandleAsync(handlerContext);
        Assert.True(handlerContext.HasFailed);
    }

    [Fact]
    public async Task ShouldReturn_Unauthorised_Result_If_Expired_Timestamp()
    {
        var httpContext = CreateMockHttpContext();

        var expiredTime = _requestTime.AddMinutes(
            SignedRequestAuthorisationPolicy.RequestTimeToLiveMinutes + 1
        );
        _systemTime.UtcNow.Returns(expiredTime);

        var handlerContext = CreateHandlerContext(httpContext);
        await _authorisationPolicy.HandleAsync(handlerContext);
        Assert.True(handlerContext.HasFailed);
    }

    [Fact]
    public async Task ShouldReturn_Unauthorised_Result_If_Incorrect_SigningSecret()
    {
        var config = new SigningSecretConfiguration() { SigningSecret = "incorrect-secret" };
        var authHandler = new SignedRequestAuthorisationPolicy(_systemTime, config, _logger);

        var httpContext = CreateMockHttpContext();
        var handlerContext = CreateHandlerContext(httpContext);
        await authHandler.HandleAsync(handlerContext);
        Assert.True(handlerContext.HasFailed);
    }

    [Fact]
    public async Task Should_Continue_Authorised_If_Valid_SigningSecret()
    {
        var httpContext = CreateMockHttpContext();
        var handlerContext = CreateHandlerContext(httpContext);
        await _authorisationPolicy.HandleAsync(handlerContext);
        Assert.True(handlerContext.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateHandlerContext(HttpContext httpContext)
    {
        IEnumerable<IAuthorizationRequirement> requirements =
        [
            new SignedRequestAuthorisationRequirement(),
        ];
        var user = httpContext.User;
        return new AuthorizationHandlerContext(requirements, user, httpContext);
    }

    private DefaultHttpContext CreateMockHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(
            SignedRequestAuthorisationPolicy.HeaderSignedValues,
            MockHeaderSignedValues
        );
        httpContext.Request.Headers.Append(
            SignedRequestAuthorisationPolicy.HeaderSignature,
            CorrectSignature
        );
        httpContext.Request.Headers.Append(
            SignedRequestAuthorisationPolicy.HeaderTimestamp,
            _requestTimestamp
        );
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(MockRequestBody));
        httpContext.Request.Path = new PathString("/api/cms/webhook");
        httpContext.Request.Method = "POST";

        return httpContext;
    }
}

using System.Text;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Web.Authorisation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Authorisation;

public class SigningSecretAuthorisationFilterTests
{
    private const string CorrectSigningSecret = "correct-signing-secret";
    private const string MockHeaderSignedValues = "x-contentful-signed-headers,x-contentful-timestamp";
    private const string MockRequestBody = "{\"body\":\"something\"}";
    private const string CorrectSignature = "2370b988c5585881053ab2bd478e0ab45178b3779642de50eaad0a1676d65bf0";

    private readonly DateTime _requestTime;
    private readonly string _requestTimestamp;

    private readonly SigningSecretAuthorisationFilter _authorisationFilter;
    private readonly ILogger<SigningSecretAuthorisationFilter> _logger = Substitute.For<ILogger<SigningSecretAuthorisationFilter>>();
    private readonly ISystemTime _systemTime = Substitute.For<ISystemTime>();

    public SigningSecretAuthorisationFilterTests()
    {
        var config = new SigningSecretConfiguration() { SigningSecret = CorrectSigningSecret };
        _authorisationFilter = new SigningSecretAuthorisationFilter(_systemTime, config, _logger);

        _requestTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        _requestTimestamp = new DateTimeOffset(_requestTime).ToUnixTimeMilliseconds().ToString();
        _systemTime.UtcNow.Returns(_ => _requestTime);
    }

    [Fact]
    public void ShouldReturn_Unauthorised_Result_If_Missing_Headers()
    {
        var httpContext = new DefaultHttpContext();
        var filterContext = CreateFilterContext(httpContext);
        _authorisationFilter.OnAuthorization(filterContext);
        Assert.IsType<UnauthorizedResult>(filterContext.Result);
    }

    [Fact]
    public void ShouldReturn_Unauthorised_Result_If_Expired_Timestamp()
    {
        var httpContext = CreateMockHttpContext();

        var expiredTime = _requestTime.AddMinutes(SigningSecretAuthorisationFilter.RequestTimeToLiveMinutes + 1);
        _systemTime.UtcNow.Returns(expiredTime);

        var filterContext = CreateFilterContext(httpContext);
        _authorisationFilter.OnAuthorization(filterContext);
        Assert.IsType<UnauthorizedResult>(filterContext.Result);
    }

    [Fact]
    public void ShouldReturn_Unauthorised_Result_If_Incorrect_SigningSecret()
    {
        var config = new SigningSecretConfiguration() { SigningSecret = "incorrect-secret" };
        var authFilter = new SigningSecretAuthorisationFilter(_systemTime, config, _logger);

        var httpContext = CreateMockHttpContext();
        var filterContext = CreateFilterContext(httpContext);
        authFilter.OnAuthorization(filterContext);
        Assert.IsType<UnauthorizedResult>(filterContext.Result);
    }

    [Fact]
    public void Should_Continue_Authorised_If_Valid_SigningSecret()
    {
        var httpContext = CreateMockHttpContext();
        var filterContext = CreateFilterContext(httpContext);
        _authorisationFilter.OnAuthorization(filterContext);
        Assert.IsNotType<UnauthorizedResult>(filterContext.Result);
        Assert.Null(filterContext.Result);
    }

    private static AuthorizationFilterContext CreateFilterContext(HttpContext httpContext)
    {
        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };
        return new AuthorizationFilterContext(actionContext, []);
    }

    private DefaultHttpContext CreateMockHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(SigningSecretAuthorisationFilter.HeaderSignedValues, MockHeaderSignedValues);
        httpContext.Request.Headers.Append(SigningSecretAuthorisationFilter.HeaderSignature, CorrectSignature);
        httpContext.Request.Headers.Append(SigningSecretAuthorisationFilter.HeaderTimestamp, _requestTimestamp);
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(MockRequestBody));
        httpContext.Request.Path = new PathString("/api/cms/webhook");
        httpContext.Request.Method = "POST";

        return httpContext;
    }
}

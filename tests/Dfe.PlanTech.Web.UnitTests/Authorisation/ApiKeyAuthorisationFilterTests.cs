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

public class ApiKeyAuthorisationFilterTests
{
    private const string KeyValue = "mock-refresh-api-key";
    private readonly ApiKeyAuthorisationFilter _authorisationFilter;
    private readonly ILogger<ApiKeyAuthorisationFilter> _logger = Substitute.For<ILogger<ApiKeyAuthorisationFilter>>();
    public ApiKeyAuthorisationFilterTests()
    {
        var config = new ApiAuthenticationConfiguration { KeyValue = KeyValue };
        _authorisationFilter = new ApiKeyAuthorisationFilter(config, _logger);
    }

    [Fact]
    public void ShouldReturn_Unauthorised_Result_If_Missing_ApiKey()
    {
        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };
        var filterContext = new AuthorizationFilterContext(actionContext, []);
        _authorisationFilter.OnAuthorization(filterContext);
        Assert.IsType<UnauthorizedResult>(filterContext.Result);
    }

    [Theory]
    [InlineData("invalid-api-key")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Bearer invalid-api-key")]
    public void ShouldReturn_Unauthorised_Result_If_Invalid_ApiKey(string? apiKey)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(ApiKeyAuthorisationFilter.AuthHeaderKey, apiKey);
        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };
        var filterContext = new AuthorizationFilterContext(actionContext, []);
        _authorisationFilter.OnAuthorization(filterContext);
        Assert.IsType<UnauthorizedResult>(filterContext.Result);
    }

    [Fact]
    public void Should_Continue_Authorised_If_Valid_ApiKey()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(ApiKeyAuthorisationFilter.AuthHeaderKey, ApiKeyAuthorisationFilter.AuthValuePrefix + KeyValue);
        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };
        var filterContext = new AuthorizationFilterContext(actionContext, []);
        _authorisationFilter.OnAuthorization(filterContext);
        Assert.IsNotType<UnauthorizedResult>(filterContext.Result);
        Assert.Null(filterContext.Result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Handle_MissingApiKeyConfiguration(string? apiKey)
    {
        var config = new ApiAuthenticationConfiguration { KeyValue = apiKey! };
        var authorisationFilter = new ApiKeyAuthorisationFilter(config, _logger);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(ApiKeyAuthorisationFilter.AuthHeaderKey, ApiKeyAuthorisationFilter.AuthValuePrefix + KeyValue);
        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };
        var filterContext = new AuthorizationFilterContext(actionContext, []);
        authorisationFilter.OnAuthorization(filterContext);
        Assert.IsType<UnauthorizedResult>(filterContext.Result);

        var loggedMessages = _logger.ReceivedLogMessages().ToArray();

        Assert.Single(loggedMessages);
        Assert.Equal($"API key {nameof(config.KeyValue)} is missing", loggedMessages[0].Message);
        Assert.Equal(LogLevel.Error, loggedMessages[0].LogLevel);
    }
}

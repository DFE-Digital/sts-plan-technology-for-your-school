using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Web.Authorisation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Authorisation;

public class ApiKeyAuthorisationFilterTests
{
    private const string RefreshEndpoint = "mock-refresh-endpoint";
    private const string RefreshApiKeyName = "X-WEBSITE-CACHE-CLEAR-API-KEY";
    private const string RefreshApiKeyValue = "mock-refresh-api-key";
    private readonly ApiKeyAuthorisationFilter _authorisationFilter;

    public ApiKeyAuthorisationFilterTests()
    {
        var cacheRefreshConfiguration = new CacheRefreshConfiguration(RefreshEndpoint, RefreshApiKeyName, RefreshApiKeyValue);
        _authorisationFilter = new ApiKeyAuthorisationFilter(cacheRefreshConfiguration);
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

    [Fact]
    public void ShouldReturn_Unauthorised_Result_If_Invalid_ApiKey()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(RefreshApiKeyName, "invalid-api-key");
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
        httpContext.Request.Headers.Append(RefreshApiKeyName, RefreshApiKeyValue);
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
}

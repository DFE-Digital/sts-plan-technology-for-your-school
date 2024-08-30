using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Dfe.PlanTech.Web.Authorisation;

/// <summary>
/// Retrieves a given <see cref="Page"/> from Contentful, and authorises the user based off that
/// </summary>
public class PageModelAuthorisationPolicy(ILogger<PageModelAuthorisationPolicy> logger) : AuthorizationHandler<PageAuthorisationRequirement>
{
    public const string PolicyName = "UsePageAuthentication";
    public const string RoutesValuesRouteNameKey = "route";
    public const string RouteValuesControllerNameKey = "controller";
    private const string IndexSlug = "/";
    private readonly ILogger<PageModelAuthorisationPolicy> _logger;

    public PageModelAuthorisationPolicy(ILogger<PageModelAuthorisationPolicy> logger)
    {
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PageAuthorisationRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext)
        {
            _logger.LogError("Expected resource to be HttpContext but received {type}", context.Resource?.GetType());
            return;
        }

        bool success = await ProcessPage(httpContext);

        if (success)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }

    private static async Task<bool> ProcessPage(HttpContext httpContext)
    {
        string? slug = GetRequestRoute(httpContext);

        if (slug == null)
        {
            return UserIsAuthenticated(httpContext);
        }

        using var scope = httpContext.RequestServices.CreateAsyncScope();
        var pageQuery = scope.ServiceProvider.GetRequiredService<IGetPageQuery>();

        Page page = await GetPageForSlug(httpContext, slug, pageQuery) ?? throw new KeyNotFoundException($"Could not find page for {slug}");

        httpContext.Items.Add(nameof(Page), page);

        return !page.RequiresAuthorisation || UserIsAuthenticated(httpContext);
    }

    private static async Task<Page> GetPageForSlug(HttpContext httpContext, string slug, IGetPageQuery pageQuery)
    => await pageQuery.GetPageBySlug(slug, httpContext.RequestAborted) ?? throw new KeyNotFoundException($"Could not find page with slug {slug}");

    private static bool UserIsAuthenticated(HttpContext httpContext) => httpContext.User.Identity?.IsAuthenticated == true;

    private static string? GetRequestRoute(HttpContext httpContext)
    {
        var controllerName = httpContext.Request.RouteValues[RouteValuesControllerNameKey];
        if (controllerName == null || controllerName is not string controllerString || controllerString != PagesController.ControllerName)
        {
            return null;
        }

        var slug = httpContext.Request.RouteValues[RoutesValuesRouteNameKey];

        if (slug is not string slugString)
        {
            httpContext.Request.RouteValues[RoutesValuesRouteNameKey] = IndexSlug;
            return IndexSlug;
        }

        return slugString;
    }
}


public class PageAuthorisationRequirement : IAuthorizationRequirement
{
    public PageAuthorisationRequirement()
    {

    }
}

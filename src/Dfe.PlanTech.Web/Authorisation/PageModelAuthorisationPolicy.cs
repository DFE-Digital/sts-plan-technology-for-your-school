using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Microsoft.AspNetCore.Authorization;

namespace Dfe.PlanTech.Web.Authorisation;

/// <summary>
/// Retrieves a given <see cref="Page"/> from Contentful, and authorises the user based off that 
/// </summary>
public class PageModelAuthorisationPolicy : AuthorizationHandler<PageAuthorisationRequirement>
{
    public const string POLICY_NAME = "UsePageAuthentication";
    public const string ROUTE_NAME = "route";

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
        string slug = GetRequestRoute(httpContext);

        using var scope = httpContext.RequestServices.CreateAsyncScope();
        var pageQuery = scope.ServiceProvider.GetRequiredService<IGetPageQuery>();
        var authQuery = scope.ServiceProvider.GetRequiredService<GetPageAuthenticationQuery>();


        var requiresAuth = await authQuery.PageRequiresAuthentication(slug, CancellationToken.None);

        return !requiresAuth || UserIsAuthorised(httpContext, requiresAuth);
    }

    private static async Task<Page> GetPageForSlug(HttpContext httpContext, string slug, IGetPageQuery pageQuery)
    => await pageQuery.GetPageBySlug(slug, httpContext.RequestAborted) ?? throw new KeyNotFoundException($"Could not find page with slug {slug}");

    private static bool UserIsAuthorised(HttpContext httpContext, bool requiredAuth)
    => requiredAuth && httpContext.User.Identity?.IsAuthenticated == true;

    private static string GetRequestRoute(HttpContext httpContext)
    {
        var slug = httpContext.Request.RouteValues[ROUTE_NAME];

        if (slug is not string slugString)
        {
            slugString = "/";
            httpContext.Request.RouteValues[ROUTE_NAME] = slugString;
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

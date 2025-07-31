using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Infrastructure.SignIns.Extensions;
using Dfe.PlanTech.Infrastructure.SignIns.Models;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Dfe.PlanTech.Web.Binders;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Dfe.PlanTech.Web.Authorisation.Policies;

/// <summary>
/// Checks user authorisation for the current page, and retrieves a given <see cref="Page"/> from Contentful if needed for the request.
/// </summary>
public class PageModelAuthorisationPolicy(
    ILoggerFactory loggerFactory
) : AuthorizationHandler<PageAuthorisationRequirement>
{
    private readonly ILogger<PageModelAuthorisationPolicy> _logger = loggerFactory.CreateLogger<PageModelAuthorisationPolicy>();

    private const string IndexSlug = "/";
    public const string PolicyName = "UsePageAuthentication";
    public const string RouteValuesActionNameKey = "action";
    public const string RouteValuesControllerNameKey = "controller";
    public const string RoutesValuesRouteNameKey = "route";

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PageAuthorisationRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext)
        {
            _logger.LogError("Expected resource to be HttpContext but received {type}", context.Resource?.GetType());
            return;
        }

        var userAuthorisationResult = await GetUserAuthorisationResult(httpContext);
        httpContext.Items.Add(UserAuthorisationResult.HttpContextKey, userAuthorisationResult);

        if (userAuthorisationResult.AuthenticationMatches)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }

    /// <summary>
    /// Checks whether the user's authentication matches the requirements for the route
    /// </summary>
    /// <remarks>
    /// Process:
    /// 1. Is the request to the PagesController?
    ///    - If not, return whether they are authenticated or not
    /// 2. If it is the pages controller, retrieve the relevant page for the request from DB/Contentful
    /// 3. Return whether the user's authentication status matches the page's authentication requirements
    ///    - Page does not require authentication? Then return true
    ///    - Page requires authentication? Return whether the user is authenticated or not
    /// </remarks>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    private async Task<UserAuthorisationResult> GetUserAuthorisationResult(HttpContext httpContext)
    {
        var userAuthorisationStatus = httpContext.User.AuthorisationStatus();

        string? slug = GetRequestRoute(httpContext);

        if (slug == null)
        {
            return new UserAuthorisationResult(true, userAuthorisationStatus);
        }

        CmsPageDto? page = await GetPageForSlug(httpContext, slug);

        if (page == null)
        {
            return new UserAuthorisationResult(false, userAuthorisationStatus);
        }

        return new UserAuthorisationResult(page.RequiresAuthorisation, userAuthorisationStatus);
    }

    /// <summary>
    /// Retrieves the relevant page from Contentful/DB, and adds it to the HttpContext.
    /// </summary>
    /// <remarks>
    /// The page ias added to the HttpContext for use in the <see cref="PageModelBinder"/>, 
    /// to prevent the page being loaded multiple times for a single request
    /// </remarks>
    private async Task<CmsPageDto?> GetPageForSlug(HttpContext httpContext, string slug)
    {
        using var scope = httpContext.RequestServices.CreateAsyncScope();
        var contentfulService = scope.ServiceProvider.GetRequiredService<ContentfulService>();
        var page = await contentfulService.GetPageBySlugAsync(slug);
        httpContext.Items.Add(nameof(CmsPageDto), page);

        return page;
    }

    /// <summary>
    /// Gets the slug from the route if on the pages controller, or null if not.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    private string? GetRequestRoute(HttpContext httpContext)
    {
        if (!ControllerIsPagesController(httpContext))
        {
            _logger.LogTrace("Request is not from/to the Pages controller. Request is to controller {ControllerName} and action {ActioName}",
                            httpContext.Request.RouteValues[RouteValuesControllerNameKey],
                            httpContext.Request.RouteValues[RouteValuesActionNameKey]);

            return null;
        }

        return GetSlugFromRoute(httpContext);
    }

    /// <summary>
    /// Gets the slug of the page from the request route values, or the index page slug if none is found
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    private string GetSlugFromRoute(HttpContext httpContext)
    {
        var slug = httpContext.Request.RouteValues[RoutesValuesRouteNameKey];

        if (slug is not string slugString)
        {
            _logger.LogTrace("Route is null - request is to index page");
            httpContext.Request.RouteValues[RoutesValuesRouteNameKey] = IndexSlug;
            return IndexSlug;
        }

        _logger.LogTrace("Route slug for request is {Slug}", slug);
        return slugString;
    }

    /// <summary>
    /// Checks whether the request is from the PagesController
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    private static bool ControllerIsPagesController(HttpContext httpContext)
    {
        var controllerName = httpContext.Request.RouteValues[RouteValuesControllerNameKey];

        return controllerName is string controllerString && controllerString == PagesController.ControllerName;
    }
}

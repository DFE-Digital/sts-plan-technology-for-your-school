using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Infrastructure.SignIn.Extensions;
using Dfe.PlanTech.Infrastructure.SignIn.Models;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Dfe.PlanTech.Web.Binders;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Dfe.PlanTech.Web.Authorisation.Policies;

/// <summary>
/// Checks user authorisation for the current page, and retrieves a given <see cref="Page"/> from Contentful if needed for the request.
/// </summary>
public class PageModelAuthorisationPolicy(
    ILogger<PageModelAuthorisationPolicy> logger
) : AuthorizationHandler<PageAuthorisationRequirement>
{
    private const string IndexSlug = "/";
    public const string PolicyName = "UsePageAuthentication";
    public const string RouteValuesActionNameKey = "action";
    public const string RouteValuesControllerNameKey = "controller";
    public const string RoutesValuesRouteNameKey = "route";

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PageAuthorisationRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext)
        {
            logger.LogError("Expected resource to be HttpContext but received {Type}", context.Resource?.GetType());
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
        var userAuthorisationStatus = httpContext.User.GetAuthorisationStatus();

        if (!ControllerIsPagesController(httpContext))
        {
            logger.LogTrace("Request is not from/to the Pages controller. Request is to controller {ControllerName} and action {ActioName}",
                httpContext.Request.RouteValues[RouteValuesControllerNameKey],
                httpContext.Request.RouteValues[RouteValuesActionNameKey]);

            return new UserAuthorisationResult(PageRequiresAuthorisation: true, userAuthorisationStatus);
        }

        string slug = GetSlugFromRoute(httpContext);
        try
        {
            var page = await GetPageForSlug(httpContext, slug);
            return new UserAuthorisationResult(PageRequiresAuthorisation: page.RequiresAuthorisation, userAuthorisationStatus);
        }
        catch (ContentfulDataUnavailableException e)
        {
            // Pages which do not have corresponding Contentful entries do not require authorisation(?)
            logger.LogWarning(e, "Could not retrieve page from Contentful for slug {Slug} (not found) therefore unable to determine authorisation requirements, defaulting to allowing access", slug);
            return new UserAuthorisationResult(PageRequiresAuthorisation: false, userAuthorisationStatus);
        }
        catch (Exception e)
        {
            // Every other error should allow access(?)
            logger.LogError(e, "Could not retrieve page from Contentful for slug {Slug}, unable to determine authorisation requirements, defaulting to allowing access", slug);
            return new UserAuthorisationResult(PageRequiresAuthorisation: false, userAuthorisationStatus);
        }
    }

    /// <summary>
    /// Retrieves the relevant page from Contentful/DB, and adds it to the HttpContext.
    /// </summary>
    /// <remarks>
    /// The page ias added to the HttpContext for use in the <see cref="PageModelBinder"/>,
    /// to prevent the page being loaded multiple times for a single request
    /// </remarks>
    private static async Task<PageEntry> GetPageForSlug(HttpContext httpContext, string slug)
    {
        using var scope = httpContext.RequestServices.CreateAsyncScope();
        var contentfulService = scope.ServiceProvider.GetRequiredService<IContentfulService>();
        var page = await contentfulService.GetPageBySlugAsync(slug);
        httpContext.Items.Add(nameof(PageEntry), page);

        return page;
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
            logger.LogTrace("Route is null - request is to index page");
            httpContext.Request.RouteValues[RoutesValuesRouteNameKey] = IndexSlug;
            return IndexSlug;
        }

        logger.LogTrace("Route slug for request is {Slug}", slug);
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

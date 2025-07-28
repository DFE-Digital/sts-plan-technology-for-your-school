using Dfe.PlanTech.Infrastructure.SignIns.Models;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Dfe.PlanTech.Web.Authorisation.Handlers;

/// <summary>
/// Checks user authorisation for the current page, and retrieves a given <see cref="Page"/> from Contentful if needed for the request.
/// </summary>
public class UserOrganisationAuthorisationHandler(
    ILoggerFactory loggerFactory
) : AuthorizationHandler<UserOrganisationAuthorisationRequirement>
{
    private readonly ILogger<UserOrganisationAuthorisationHandler> _logger = loggerFactory.CreateLogger<UserOrganisationAuthorisationHandler>();

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserOrganisationAuthorisationRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext)
        {
            _logger.LogError("Expected resource to be HttpContext but received {type}", context.Resource?.GetType());
            return Task.CompletedTask;
        }

        var userAuthorisationResult = TryGetUserAuthorisationResult(httpContext);

        if (userAuthorisationResult == null || !userAuthorisationResult.PageRequiresAuthorisation || userAuthorisationResult.CanViewPage || RequestIsSignoutUrl(httpContext))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, "Missing organisation"));
        }

        return Task.CompletedTask;
    }

    private static UserAuthorisationResult? TryGetUserAuthorisationResult(HttpContext context)
    {
        if (context.Items.TryGetValue(UserAuthorisationResult.HttpContextKey, out object? userAuthorisationResult) && userAuthorisationResult is UserAuthorisationResult result)
        {
            return result;
        }

        return null;
    }

    private static bool RequestIsSignoutUrl(HttpContext context) => context.Request.Path.HasValue && context.Request.Path.Value == "/auth/sign-out";
}

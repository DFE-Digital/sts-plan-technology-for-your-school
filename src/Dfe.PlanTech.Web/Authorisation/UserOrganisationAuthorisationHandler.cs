using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.SignIns.Models;
using Microsoft.AspNetCore.Authorization;

namespace Dfe.PlanTech.Web.Authorisation;

/// <summary>
/// Checks user authorisation for the current page, and retrieves a given <see cref="Page"/> from Contentful if needed for the request.
/// </summary>
public class UserOrganisationAuthorisationHandler(ILogger<UserOrganisationAuthorisationHandler> logger) : AuthorizationHandler<UserOrganisationAuthorisationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserOrganisationAuthorisationRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext)
        {
            logger.LogError("Expected resource to be HttpContext but received {type}", context.Resource?.GetType());
            return Task.CompletedTask;
        }

        var userAuthorisationResult = TryGetUserAuthorisationResult(httpContext);

        if (userAuthorisationResult == null || !userAuthorisationResult.PageRequiresAuthorisation)
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
}

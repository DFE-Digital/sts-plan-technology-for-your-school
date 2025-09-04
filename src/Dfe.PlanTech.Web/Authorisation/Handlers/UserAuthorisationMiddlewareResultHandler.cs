using Dfe.PlanTech.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Dfe.PlanTech.Web.Authorisation.Handlers;

public class UserAuthorisationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            var redirectUrl = GetRedirectUrl(authorizeResult.AuthorizationFailure);

            if (redirectUrl != null)
            {
                context.Response.Redirect(redirectUrl);
                return;
            }
        }

        await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    public static string? GetRedirectUrl(AuthorizationFailure? authorisationFailure)
    {
        if (authorisationFailure == null)
        {
            return null;
        }

        bool userMissingOrganisation = UserMissingOrganisation(authorisationFailure);

        return userMissingOrganisation ? UrlConstants.OrgErrorPage : null;
    }

    private static bool UserMissingOrganisation(AuthorizationFailure authorisationFailure)
    => authorisationFailure.FailureReasons.Select(reason => reason.Handler).OfType<UserOrganisationAuthorisationHandler>().Any();
}

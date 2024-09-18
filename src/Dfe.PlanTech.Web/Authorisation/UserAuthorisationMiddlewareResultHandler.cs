using Dfe.PlanTech.Application.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Dfe.PlanTech.Web.Authorisation
{
  public class UserAuthorisationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
  {
    private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authoriseResult)
    {
      // If the authorization was forbidden and the resource had a specific requirement,
      // provide a custom 404 response.
      if (authoriseResult.Forbidden)
      {
        var redirectUrl = GetRedirectUrl(authoriseResult.AuthorizationFailure, context);

        if (redirectUrl != null)
        {
          context.Response.Redirect(redirectUrl);
          return;
        }
      }

      // Fall back to the default implementation.
      await defaultHandler.HandleAsync(next, context, policy, authoriseResult);
    }


    public static string? GetRedirectUrl(AuthorizationFailure? authorisationFailure, HttpContext context)
    {
      if (authorisationFailure == null)
      {
        return null;
      }

      var isSignoutUrl = RequestIsSignoutUrl(context);

      if (isSignoutUrl)
      {
        return null;
      }

      bool userMissingOrganisation = UserMissingOrganisation(authorisationFailure);

      return userMissingOrganisation ? UrlConstants.OrgErrorPage : null;
    }

    private static bool UserMissingOrganisation(AuthorizationFailure authorisationFailure)
    => authorisationFailure.FailureReasons.Select(reason => reason.Handler).OfType<UserOrganisationAuthorisationHandler>().Any();

    private static bool RequestIsSignoutUrl(HttpContext context) => context.Request.Path.HasValue && context.Request.Path.Value == "/auth/sign-out";
  }
}
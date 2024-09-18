
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Infrastructure.SignIns.Models;

namespace Dfe.PlanTech.Web.Authorisation;

public class UserAuthorizationRedirectMiddleware(RequestDelegate next)
{
  public async Task InvokeAsync(HttpContext context)
  {
    UserAuthorisationResult? authResult = TryGetUserAuthorisationResult(context);
    if (authResult != null)
    {
      var redirectUrl = GetRedirectUrl(authResult, context);
      if (redirectUrl != null)
      {
        context.Response.Redirect(redirectUrl);
        return;
      }
    }

    await next(context);
  }

  private static UserAuthorisationResult? TryGetUserAuthorisationResult(HttpContext context)
  {
    if (context.Items.TryGetValue(UserAuthorisationResult.HttpContextKey, out object? userAuthorisationResult) && userAuthorisationResult is UserAuthorisationResult result)
    {
      return result;
    }

    return null;
  }

  public static string? GetRedirectUrl(UserAuthorisationResult authResult, HttpContext context)
  {
    var userMissingOrganisation = UserMissingOrganisation(authResult);
    var isSignoutUrl = RequestIsSignoutUrl(context);

    if (userMissingOrganisation && !isSignoutUrl)
    {
      return UrlConstants.OrgErrorPage;
    }

    return null;
  }

  private static bool UserMissingOrganisation(UserAuthorisationResult authResult) => authResult.PageRequiresAuthorisation && authResult.AuthenticationMatches && !authResult.UserAuthorisationStatus.HasOrganisation;
  private static bool RequestIsSignoutUrl(HttpContext context) => context.Request.Path.HasValue && context.Request.Path.Value == "/auth/sign-out";
}


using Dfe.PlanTech.Domain.SignIns.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.SignIns;

public static class DfeOpenIdConnectEvents
{
    private const string ForwardHostHeader = "X-Forwarded-Host";

    /// <summary>
    /// Re-writes the login callback URI
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Task OnRedirectToIdentityProvider(RedirectContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IDfeSignInConfiguration>();

        context.ProtocolMessage.RedirectUri = CreateCallbackUrl(context, config, config.CallbackUrl);

        return Task.FromResult(0);
    }

    /// <summary>
    /// Re-writes the signout redirect URI
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        if (context.ProtocolMessage != null)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IDfeSignInConfiguration>();

            context.ProtocolMessage.PostLogoutRedirectUri = CreateCallbackUrl(context, config, config.SignoutRedirectUrl);
        }

        return Task.FromResult(0);
    }


    public static string GetOriginUrl(RedirectContext context, IDfeSignInConfiguration config)
    {
        var forwardHostHeader = context.HttpContext.Request.Headers
                                                            .Where(header => string.Equals(ForwardHostHeader, header.Key, StringComparison.InvariantCultureIgnoreCase))
                                                            .Select(header => header.Value.FirstOrDefault())
                                                            .FirstOrDefault();

        return forwardHostHeader ?? config.FrontDoorUrl;
    }

    public static string CreateCallbackUrl(RedirectContext context, IDfeSignInConfiguration config, string callbackPath)
    {
        var originUrl = GetOriginUrl(context, config);

        //Our config uses URls like "/auth/cb" for the redirect callback slug, signout slug, etc.
        //So we should ensure that the URL does not end with a forward slash when returning
        if(originUrl.EndsWith('/') && callbackPath.StartsWith('/')){
            originUrl = originUrl[..^1];
        }

        return $"{originUrl}{callbackPath}";
    }
}

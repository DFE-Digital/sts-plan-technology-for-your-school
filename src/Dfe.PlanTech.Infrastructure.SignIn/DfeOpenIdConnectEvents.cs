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

        var originUrl = GetOriginUrl(context, config);

        context.ProtocolMessage.RedirectUri = $"{originUrl}{config.CallbackUrl}";

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

            var originUrl = GetOriginUrl(context, config);

            context.ProtocolMessage.PostLogoutRedirectUri = $"{originUrl}{config.SignoutRedirectUrl}";
        }

        return Task.FromResult(0);
    }


    public static string GetOriginUrl(RedirectContext context, IDfeSignInConfiguration config)
    {
        var forwardHostHeader = context.HttpContext.Request.Headers
                                                            .Where(header => string.Equals(ForwardHostHeader, header.Key, StringComparison.InvariantCultureIgnoreCase))
                                                            .Select(header => header.Value.FirstOrDefault())
                                                            .FirstOrDefault();

        if (forwardHostHeader != null)
        {
            return $"https://{forwardHostHeader}/";
        }

        return config.FrontDoorUrl;
    }
}

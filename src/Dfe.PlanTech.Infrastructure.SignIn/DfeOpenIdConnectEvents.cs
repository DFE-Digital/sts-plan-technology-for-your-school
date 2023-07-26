using Dfe.PlanTech.Domain.SignIn.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.SignIn;

public static class DfeOpenIdConnectEvents
{
    /// <summary>
    /// Re-writes the login callback URI
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Task OnRedirectToIdentityProvider(RedirectContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IDfeSignInConfiguration>();

        context.ProtocolMessage.RedirectUri = $"{config.FrontDoorUrl}{config.CallbackUrl}";

        return Task.FromResult(0);
    }

    /// <summary>
    /// Re-writes the signout redirect URI
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IDfeSignInConfiguration>();

        if (context.ProtocolMessage != null)
        {
            context.ProtocolMessage.PostLogoutRedirectUri = $"{config.FrontDoorUrl}{config.SignoutRedirectUrl}";
        }

        return Task.FromResult(0);
    }
}

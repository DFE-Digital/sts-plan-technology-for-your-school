using System.Security.Claims;
using Dfe.PlanTech.Application.SignIn.Interfaces;
using Dfe.PlanTech.Domain.SignIn.Enums;
using Dfe.PlanTech.Domain.SignIn.Models;
using Dfe.PlanTech.Infrastructure.SignIn.Extensions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.SignIn;

public static class DfeOpenIdConnectEvents
{
    /// <summary>
    /// Runs once a user's token is validated; adds a user's role claims from DFE Public API
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task OnTokenValidated(TokenValidatedContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IDfeSignInConfiguration>();

        if (config.DiscoverRolesWithPublicApi)
        {
            await AddRoleClaimsFromDfePublicApi(context);
        }
    }

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
            context.ProtocolMessage.PostLogoutRedirectUri =$"{config.FrontDoorUrl}{config.SignoutRedirectUrl}";
        }

        return Task.FromResult(0);
    }

    /// <summary>
    /// Retrieves claims for user from DFE and assigns them to user identity
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static async Task AddRoleClaimsFromDfePublicApi(TokenValidatedContext context)
    {
        var dfePublicApi = context.HttpContext.RequestServices.GetRequiredService<IDfePublicApi>();

        if (context.Principal?.Identity == null || !context.Principal.Identity.IsAuthenticated)
            return;

        var userId = context.Principal.GetUserId();

        var userOrganization = context.Principal.GetOrganisation();
        if (userOrganization == null)
        {
            context.Fail("User is not in an organisation.");
            return;
        }

        var userAccessToService = await dfePublicApi.GetUserAccessToService(userId, userOrganization.Id.ToString());
        if (userAccessToService == null)
        {
            // User account is not enrolled into service and has no roles.
            return;
        }

        var roleIdentity = new ClaimsIdentity(GetRoleClaims(context, userAccessToService!));
        context.Principal.AddIdentity(roleIdentity);
    }

    private static IEnumerable<Claim> GetRoleClaims(TokenValidatedContext context, UserAccessToService userAccessToService)
    => userAccessToService.Roles
                            .Where(role => role.Status.Id == 1)
                            .SelectMany(role => GetRoleClaimsForRole(context, role));

    private static IEnumerable<Claim> GetRoleClaimsForRole(TokenValidatedContext context, Role role)
    {
        yield return new Claim(ClaimConstants.RoleCode, role.Code, ClaimTypes.Role, context.Options.ClientId);
        yield return new Claim(ClaimConstants.RoleId, role.Id.ToString(), ClaimTypes.Role, context.Options.ClientId);
        yield return new Claim(ClaimConstants.RoleName, role.Name, ClaimTypes.Role, context.Options.ClientId);
        yield return new Claim(ClaimConstants.RoleNumericId, role.NumericId, ClaimTypes.Role, context.Options.ClientId);
    }
}

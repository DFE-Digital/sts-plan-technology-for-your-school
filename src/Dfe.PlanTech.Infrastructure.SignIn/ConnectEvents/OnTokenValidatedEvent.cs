using System.Security.Claims;
using Dfe.PlanTech.Application.SignIn.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.SignIn.Enums;
using Dfe.PlanTech.Domain.SignIn.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Dfe.PlanTech.Infrastructure.SignIn.Extensions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.SignIn.ConnectEvents;

public static class OnTokenValidatedEvent
{
    /// <summary>
    /// Runs once a user's token is validated; adds a user's role claims from DFE Public API
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task OnTokenValidated(TokenValidatedContext context)
    {
        await RecordUserSign(context);

        var config = context.HttpContext.RequestServices.GetRequiredService<IDfeSignInConfiguration>();

        if (config.DiscoverRolesWithPublicApi)
        {
            await AddRoleClaimsFromDfePublicApi(context);
        }
    }

    /// <summary>
    /// Records user sign in event
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static async Task RecordUserSign(TokenValidatedContext context)
    {
        if (context.Principal?.Identity == null || !context.Principal.Identity.IsAuthenticated)
        {
            context.Fail("User is not authenticated.");
            return;
        }

        var userId = context.Principal.GetUserId();
        var recordUserSignInCommand = context.HttpContext.RequestServices.GetRequiredService<IRecordUserSignInCommand>();

        await recordUserSignInCommand.RecordSignIn(new RecordUserSignInDto()
        {
            DfeSignInRef = userId
        });
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

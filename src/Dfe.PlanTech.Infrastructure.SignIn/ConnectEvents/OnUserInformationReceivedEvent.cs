using Dfe.PlanTech.Application.SignIns.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Dfe.PlanTech.Infrastructure.SignIns.Extensions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Dfe.PlanTech.Infrastructure.SignIns.ConnectEvents;

public static class OnUserInformationReceivedEvent
{
    /// <summary>
    /// Runs once a user's info comes back from backend server - records sign in and adds a user's role claims from DFE Public API
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task OnUserInformationReceived(UserInformationReceivedContext context)
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
    private static async Task RecordUserSign(UserInformationReceivedContext context)
    {
        if (context.Principal?.Identity == null || !context.Principal.Identity.IsAuthenticated)
        {
            context.Fail("User is not authenticated.");
            return;
        }

        var userId = context.Principal.Claims.GetUserId();
        var establishment = context.Principal.Claims.GetOrganisation() ?? throw new KeyNotFoundException(ClaimConstants.Organisation);

        var recordUserSignInCommand = context.HttpContext.RequestServices.GetRequiredService<IRecordUserSignInCommand>();

        var signin = await recordUserSignInCommand.RecordSignIn(new RecordUserSignInDto()
        {
            DfeSignInRef = userId,
            Organisation = establishment
        });

        AddClaimsToPrincipal(context, signin);
    }

    private static void AddClaimsToPrincipal(UserInformationReceivedContext context, SignIn signin)
    {
        var principal = context.Principal;

        if (principal == null) return; 

        ClaimsIdentity claimsIdentity = new(new[]{
            new Claim(ClaimConstants.DB_USER_ID, signin.UserId.ToString()),
            new Claim(ClaimConstants.DB_ESTABLISHMENT_ID, signin.EstablishmentId.ToString())
        });

        principal.AddIdentity(claimsIdentity);
    }

    /// <summary>
    /// Retrieves claims for user from DFE and assigns them to user identity
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static async Task AddRoleClaimsFromDfePublicApi(UserInformationReceivedContext context)
    {
        var dfePublicApi = context.HttpContext.RequestServices.GetRequiredService<IDfePublicApi>();

        if (context.Principal?.Identity == null || !context.Principal.Identity.IsAuthenticated)
            return;

        var userId = context.Principal.Claims.GetUserId();

        var userOrganization = context.Principal.Claims.GetOrganisation();
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

    private static IEnumerable<Claim> GetRoleClaims(UserInformationReceivedContext context, UserAccessToService userAccessToService)
    => userAccessToService.Roles
                            .Where(role => role.Status.Id == 1)
                            .SelectMany(role => GetRoleClaimsForRole(context, role));

    private static IEnumerable<Claim> GetRoleClaimsForRole(UserInformationReceivedContext context, Role role)
    {
        yield return new Claim(ClaimConstants.RoleCode, role.Code, ClaimTypes.Role, context.Options.ClientId);
        yield return new Claim(ClaimConstants.RoleId, role.Id.ToString(), ClaimTypes.Role, context.Options.ClientId);
        yield return new Claim(ClaimConstants.RoleName, role.Name, ClaimTypes.Role, context.Options.ClientId);
        yield return new Claim(ClaimConstants.RoleNumericId, role.NumericId, ClaimTypes.Role, context.Options.ClientId);
    }
}

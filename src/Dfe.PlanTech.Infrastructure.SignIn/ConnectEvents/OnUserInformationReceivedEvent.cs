using System.Security.Claims;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;
using Dfe.PlanTech.Infrastructure.SignIns.Extensions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.Infrastructure.SignIns.ConnectEvents;

public static class OnUserInformationReceivedEvent
{
    /// <summary>
    /// Records user sign in event
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task RecordUserSignIn(UserInformationReceivedContext context)
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

    private static void AddClaimsToPrincipal(UserInformationReceivedContext context, Domain.SignIns.Models.SignIn signin)
    {
        var principal = context.Principal;

        if (principal == null)
            return;

        ClaimsIdentity claimsIdentity = new(new[]{
            new Claim(ClaimConstants.DB_USER_ID, signin.UserId.ToString()),
            new Claim(ClaimConstants.DB_ESTABLISHMENT_ID, signin.EstablishmentId.ToString())
        });

        principal.AddIdentity(claimsIdentity);
    }
}

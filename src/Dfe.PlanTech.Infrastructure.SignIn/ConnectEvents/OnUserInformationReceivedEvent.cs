using System.Security.Claims;
using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;
using Dfe.PlanTech.Infrastructure.SignIns.Extensions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.SignIns.ConnectEvents;

public static class OnUserInformationReceivedEvent
{
    /// <summary>
    /// Records user sign in event
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task RecordUserSignIn(ILogger logger, UserInformationReceivedContext context)
    {
        if (context.Principal?.Identity == null || !context.Principal.Identity.IsAuthenticated)
        {
            context.Fail("User is not authenticated.");
            return;
        }

        var userId = context.Principal.Claims.GetUserId();
        var establishment = context.Principal.Claims.GetOrganisation();
        var recordUserSignInCommand = context.HttpContext.RequestServices.GetRequiredService<SignInRepository>();

        if (establishment == null)
        {
            logger.LogWarning("User {UserId} is authenticated but has no establishment", userId);
            await recordUserSignInCommand.RecordSignInUserOnly(userId);
            return;
        }

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

        if (principal == null)
            return;

        string establishmentId = (signin.EstablishmentId?.ToString()) ?? throw new ArgumentNullException(nameof(signin.EstablishmentId));

        ClaimsIdentity claimsIdentity = new(new[]{
            new Claim(ClaimConstants.DB_USER_ID, signin.UserId.ToString()),
            new Claim(ClaimConstants.DB_ESTABLISHMENT_ID, establishmentId)
        });

        principal.AddIdentity(claimsIdentity);
    }
}

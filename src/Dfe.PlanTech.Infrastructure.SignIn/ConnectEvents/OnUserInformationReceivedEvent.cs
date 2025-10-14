using System.Security.Claims;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.SignIn.Extensions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.SignIn.ConnectEvents;

public static class OnUserInformationReceivedEvent
{
    /// <summary>
    /// Records user sign in event
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task RecordUserSignIn(
        ILogger<DfeSignIn> logger,
        UserInformationReceivedContext context
    )
    {
        if (context.Principal?.Identity == null || !context.Principal.Identity.IsAuthenticated)
        {
            context.Fail("User is not authenticated.");
            return;
        }

        var dsiUserReference = context.Principal.Claims.GetDsiUserReference();
        var dsiOrganisation = context.Principal.Claims.GetDsiOrganisation();
        var signInWorkflow = context.HttpContext.RequestServices.GetRequiredService<ISignInWorkflow>();

        if (dsiOrganisation is null)
        {
            logger.LogWarning("User {UserId} is authenticated, but not linked to a DSI organisation", dsiUserReference);
            await signInWorkflow.RecordSignInUserOnly(dsiUserReference);
            return;
        }

        var signin = await signInWorkflow.RecordSignIn(dsiUserReference, dsiOrganisation);

        AddClaimsToPrincipal(context, signin.EstablishmentId, signin.UserId);
    }

    private static void AddClaimsToPrincipal(UserInformationReceivedContext context, int? signinEstablishmentId, int signinUserId)
    {
        var principal = context.Principal;
        if (principal is null)
        {
            return;
        }

        string establishmentId = (signinEstablishmentId?.ToString()) ?? throw new InvalidDataException(nameof(signinEstablishmentId));

        ClaimsIdentity claimsIdentity = new([
            new Claim(ClaimConstants.DB_USER_ID, signinUserId.ToString()),
            new Claim(ClaimConstants.DB_ESTABLISHMENT_ID, establishmentId)
        ]);

        principal.AddIdentity(claimsIdentity);
    }
}

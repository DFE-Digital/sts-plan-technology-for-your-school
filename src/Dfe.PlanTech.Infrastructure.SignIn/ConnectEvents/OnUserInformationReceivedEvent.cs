using System.Security.Claims;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
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

        var dsiReference = context.Principal.Claims.GetDsiReference();
        var establishment = context.Principal.Claims.GetOrganisation();
        var signInWorkflow = context.HttpContext.RequestServices.GetRequiredService<ISignInWorkflow>();

        if (establishment is null)
        {
            logger.LogWarning("User {UserId} is authenticated but has no establishment", dsiReference);
            await signInWorkflow.RecordSignInUserOnly(dsiReference);
            return;
        }

        var signin = await signInWorkflow.RecordSignIn(dsiReference, establishment);

        AddClaimsToPrincipal(context, signin);
    }

    private static void AddClaimsToPrincipal(UserInformationReceivedContext context, SqlSignInDto signin)
    {
        var principal = context.Principal;

        if (principal is null)
        {
            return;
        }

        string establishmentId = (signin.EstablishmentId?.ToString()) ?? throw new InvalidDataException(nameof(signin.EstablishmentId));

        ClaimsIdentity claimsIdentity = new([
            new Claim(ClaimConstants.DB_USER_ID, signin.UserId.ToString()),
            new Claim(ClaimConstants.DB_ESTABLISHMENT_ID, establishmentId)
        ]);

        principal.AddIdentity(claimsIdentity);
    }
}

using System.Security.Claims;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
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
        ILogger<IDfeSignIn> logger,
        UserInformationReceivedContext context
    )
    {
        if (context.Principal?.Identity == null || !context.Principal.Identity.IsAuthenticated)
        {
            context.Fail("User is not authenticated.");
            return;
        }

        var dsiUserReference = context.Principal.Claims.GetDsiReference();
        if (dsiUserReference is null)
        {
            logger.LogError("Authentication failed: no nameidentifier claim found for user.");
            context.Fail("No nameidentifier claim present in user principal.");
            return;
        }

        var dsiUserOrganisation = context.Principal.Claims.GetOrganisation();
        var signInService =
            context.HttpContext.RequestServices.GetRequiredService<ISignInService>();

        if (dsiUserOrganisation is null)
        {
            logger.LogWarning(
                "User {UserId} is authenticated but has no establishment",
                dsiUserReference
            );
            await signInService.RecordSignInUserOnlyAsync(dsiUserReference);
            return;
        }

        var signin = await signInService.RecordSignInAsync(dsiUserReference, dsiUserOrganisation);

        if (
            dsiUserOrganisation.Category != null
            && DsiConstants.SatOrganisationCategoryIds.Contains(dsiUserOrganisation.Category.Id)
        )
        {
            var giasService =
                context.HttpContext.RequestServices.GetRequiredService<IGiasService>();
            var establishmentId = await giasService.GetSchoolEstablishmentIdForSatAsync(
                dsiUserOrganisation
            );

            if (establishmentId is not null)
            {
                signin.EstablishmentId = establishmentId;
            }
        }

        AddClaimsToPrincipal(context, signin);
    }

    private static void AddClaimsToPrincipal(
        UserInformationReceivedContext context,
        SqlSignInDto signin
    )
    {
        var principal = context.Principal;
        if (principal is null)
        {
            return;
        }

        string establishmentId =
            (signin.EstablishmentId?.ToString())
            ?? throw new InvalidDataException(nameof(signin.EstablishmentId));

        ClaimsIdentity claimsIdentity = new([
            new Claim(ClaimConstants.DB_USER_ID, signin.UserId.ToString()),
            new Claim(ClaimConstants.DB_ESTABLISHMENT_ID, establishmentId),
            new Claim(ClaimConstants.SessionId, Guid.NewGuid().ToString()),
        ]);

        principal.AddIdentity(claimsIdentity);
    }
}

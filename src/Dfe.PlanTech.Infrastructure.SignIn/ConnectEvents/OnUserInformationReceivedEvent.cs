using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Application.Workflows.Interfaces;
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

        var dsiReference = context.Principal.Claims.GetDsiReference();

        if (dsiReference is null)
        {
            logger.LogError("Authentication failed: no nameidentifier claim found for user.");
            context.Fail("No nameidentifier claim present in user principal.");
            return;
        }

        var organisation = context.Principal.Claims.GetOrganisation();
        var signInWorkflow =
            context.HttpContext.RequestServices.GetRequiredService<ISignInWorkflow>();

        if (organisation is null)
        {
            logger.LogWarning(
                "User {UserId} is authenticated but has no establishment",
                dsiReference
            );
            await signInWorkflow.RecordSignInUserOnly(dsiReference);
            return;
        }

        (EstablishmentModel updatedOrganisation, SqlSignInDto signin) =
            await signInWorkflow.RecordSignIn(dsiReference, organisation);

        if (updatedOrganisation != null && updatedOrganisation.Urn != organisation.Urn)
        {
            // Remove the trust organisation from the cookie and replace it with the school organisation
            ReplaceOrganisationClaim(context, organisation, updatedOrganisation);
        }

        AddClaimsToPrincipal(context, signin);
    }

    private static void AddClaimsToPrincipal(
        UserInformationReceivedContext context,
        SqlSignInDto signin
    )
    {
        var principal = context.Principal!;

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

    private static void ReplaceOrganisationClaim(
        UserInformationReceivedContext context,
        EstablishmentModel originalOrganisation,
        EstablishmentModel updatedOrganisation
    )
    {
        var principal = context.Principal!;
        var identity = (ClaimsIdentity)principal.Identity!;

        var existingClaim = identity.FindFirst(ClaimConstants.Organisation);
        if (existingClaim != null)
        {
            var newOrganisationJson = JsonSerializer.Serialize(updatedOrganisation);
            ClaimsIdentity claimsIdentity = new([
                new Claim(ClaimConstants.Organisation, newOrganisationJson),
            ]);

            identity.RemoveClaim(existingClaim);
            principal.AddIdentity(claimsIdentity);
        }
    }
}

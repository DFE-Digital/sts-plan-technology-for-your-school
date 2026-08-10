using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
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
        var signInWorkflow =
            context.HttpContext.RequestServices.GetRequiredService<ISignInWorkflow>();

        if (dsiUserOrganisation is null)
        {
            logger.LogWarning(
                "User {UserId} is authenticated but has no establishment",
                dsiUserReference
            );
            await signInWorkflow.RecordSignInUserOnly(dsiUserReference);
            return;
        }

        EstablishmentModel? satSchoolOrganisation = null;
        if (
            dsiUserOrganisation.Category != null
            && DsiConstants.SatOrganisationCategoryIds.Contains(dsiUserOrganisation.Category.Id)
        )
        {
            satSchoolOrganisation = await GetReplacementDsiOrganisationForSat(
                context,
                dsiUserOrganisation,
                dsiUserReference
            );
        }

        var signin = await signInWorkflow.RecordSignIn(
            dsiUserReference,
            satSchoolOrganisation ?? dsiUserOrganisation
        );

        if (satSchoolOrganisation != null)
        {
            // Add the single academy establishment to the cookie
            AddSingleAcademyOrganisationClaim(context, dsiUserOrganisation, satSchoolOrganisation);
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

    private static async Task<EstablishmentModel> GetReplacementDsiOrganisationForSat(
        UserInformationReceivedContext context,
        EstablishmentModel dsiOrganisation,
        string dsiUserReference
    )
    {
        if (dsiOrganisation.Category?.Id is null)
        {
            return dsiOrganisation;
        }

        // If the user is a (S)SAT, find the user's corresponding school.
        GiasEstablishmentEntity? school = null;
        if (int.TryParse(dsiOrganisation.Uid, out var satGroupUid))
        {
            var giasRepository =
                context.HttpContext.RequestServices.GetRequiredService<IGiasRepository>();

            school = await giasRepository.GetSingleAcademySchool(satGroupUid);
        }

        // Throw if not found
        if (school is null)
        {
            var organisationType = dsiOrganisation.Category.Id switch
            {
                DsiConstants.SatOrganisationCategoryId => "SAT",
                DsiConstants.SSatOrganisationCategoryId => "SSAT",
                _ => "unknown organisation type",
            };

            throw new InvalidOperationException(
                $"GIAS establishment not found for {organisationType} with UID '{dsiOrganisation.Uid}'"
            );
        }

        var dsiOrganisationProvider =
            context.HttpContext.RequestServices.GetRequiredService<IDsiOrganisationProvider>();

        var replacementDsiOrganisation = await dsiOrganisationProvider.GetOrganisationForUserAsync(
            dsiUserReference,
            school.Urn.ToString()
        );

        return replacementDsiOrganisation ?? dsiOrganisation;
    }

    private static void AddSingleAcademyOrganisationClaim(
        UserInformationReceivedContext context,
        EstablishmentModel originalOrganisation,
        EstablishmentModel updatedOrganisation
    )
    {
        var principal = context.Principal!;
        var newOrganisationJson = JsonSerializer.Serialize(updatedOrganisation);
        ClaimsIdentity claimsIdentity = new([
            new Claim(ClaimConstants.SINGLE_ACADEMY_ORGANISATION, newOrganisationJson),
        ]);

        principal.AddIdentity(claimsIdentity);
    }
}

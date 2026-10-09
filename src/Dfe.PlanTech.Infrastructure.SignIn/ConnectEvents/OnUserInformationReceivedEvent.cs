using System.Security.Claims;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
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

        var signin = await signInWorkflow.RecordSignIn(dsiUserReference, dsiUserOrganisation);

        AddClaimsToPrincipal(context, signin);

        if (
            dsiUserOrganisation.Category != null
            && DsiConstants.SatOrganisationCategoryIds.Contains(dsiUserOrganisation.Category.Id)
        )
        {
            await SetSelectedSchoolIfSat(context, dsiUserOrganisation);
        }
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

    private static async Task SetSelectedSchoolIfSat(
        UserInformationReceivedContext context,
        EstablishmentModel dsiOrganisation
    )
    {
        var categoryId = dsiOrganisation.Category?.Id;

        if (categoryId is null || !DsiConstants.SatOrganisationCategoryIds.Contains(categoryId))
        {
            return;
        }

        // Now we know the user is from a (S)SAT, find the user's corresponding school.
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
            var orgType = categoryId == DsiConstants.SatOrganisationCategoryId ? "SAT" : "SSAT";

            throw new InvalidGiasDataException(
                $"School not found for {orgType} with group UID '{dsiOrganisation.Uid}'"
            );
        }

        var currentUser =
            context.HttpContext.RequestServices.GetRequiredService<ICurrentUserProvider>();
        currentUser.SetGroupSelectedSchool(school.Urn.ToString(), school.EstablishmentName);
    }
}

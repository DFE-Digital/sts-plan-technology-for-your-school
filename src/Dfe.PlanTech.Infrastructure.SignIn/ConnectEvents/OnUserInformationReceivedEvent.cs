using System.Security.Claims;
using Dfe.PlanTech.Application.Providers.Interfaces;
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
        if (string.IsNullOrWhiteSpace(dsiUserReference))
        {
            logger.LogError("Authentication failed: no nameidentifier claim found for user.");
            context.Fail("No nameidentifier claim present in user principal.");
            return;
        }

        var dsiOrganisation = context.Principal.Claims.GetOrganisation();
        var signInService =
            context.HttpContext.RequestServices.GetRequiredService<ISignInService>();

        if (dsiOrganisation?.Category?.Id is null)
        {
            logger.LogWarning(
                "User {UserId} is authenticated but has no establishment",
                dsiUserReference
            );

            await signInService.RecordSignInUserOnlyAsync(dsiUserReference);
            return;
        }

        var signIn = await signInService.RecordSignInAsync(dsiUserReference, dsiOrganisation);

        var establishmentService =
            context.HttpContext.RequestServices.GetRequiredService<IEstablishmentService>();
        var sqlEstablishment = await establishmentService.GetOrCreateEstablishmentAsync(
            dsiOrganisation
        );

        var currentUser =
            context.HttpContext.RequestServices.GetRequiredService<ICurrentUserProvider>();
        currentUser.SetEstablishment(sqlEstablishment);

        if (dsiOrganisation.Category.Id == DsiConstants.EstablishmentCategoryId)
        {
            await ProcessSchoolLoginAsync(logger, currentUser, dsiOrganisation, dsiUserReference);
        }
        else if (DsiConstants.SatOrganisationCategoryIds.Contains(dsiOrganisation.Category.Id))
        {
            await ProcessSatLoginAsync(
                logger,
                context,
                currentUser,
                signIn,
                dsiOrganisation,
                dsiUserReference
            );
        }
        else if (
            DsiConstants.SelectASchoolOrganisationCategoryIds.Contains(dsiOrganisation.Category.Id)
        )
        {
            await ProcessGroupLogin(
                logger,
                context,
                currentUser,
                dsiOrganisation,
                dsiUserReference
            );
        }

        // Break here to check that signIn has correct establishmentId for SATs.
        AddClaimsToPrincipal(context, signIn);
    }

    private static async Task ProcessSchoolLoginAsync(
        ILogger<IDfeSignIn> logger,
        ICurrentUserProvider currentUser,
        EstablishmentModel dsiOrganisation,
        string dsiUserReference
    )
    {
        if (string.IsNullOrWhiteSpace(dsiOrganisation.Urn))
        {
            logger.LogWarning(
                "No URN found in DSI data for school {SchoolName} when user with nameidentifier {DsiReference} signed in",
                dsiOrganisation.Name,
                dsiUserReference
            );
        }

        await currentUser.SetFocusedSchoolAsync(dsiOrganisation.Reference, dsiOrganisation.Name);
    }

    private static async Task ProcessSatLoginAsync(
        ILogger<IDfeSignIn> logger,
        UserInformationReceivedContext context,
        ICurrentUserProvider currentUser,
        SqlSignInDto signIn,
        EstablishmentModel dsiOrganisation,
        string dsiUserReference
    )
    {
        // SATs and SSATs:
        // Attempt to find single school within trust. Warn if not found.
        // Set focused school.
        var satEstablishment = await GetSingleAcademySchoolEstablishmentAsync(
            context,
            dsiOrganisation
        );
        if (satEstablishment is null)
        {
            var additionalWordSecure = dsiOrganisation.Category!.Id switch
            {
                DsiConstants.SatOrganisationCategoryId => " ",
                DsiConstants.SSatOrganisationCategoryId => "secure ",
                _ => throw new InvalidDataException(
                    "Should not be able to reach this code without being a SAT or SSAT"
                ),
            };

            logger.LogWarning(
                "No school found in GIAS for {Secure}single-academy login by user with nameidentifier {DsiReference} and group UID: {Uid}",
                additionalWordSecure,
                dsiUserReference,
                dsiOrganisation.GroupUid
            );
        }
        else
        {
            // If we found the (S)SAT's school, override the establishmentId in the claims.
            // We recorded the SAT in the sign-in record, but we will treat the user as being from
            // a school from now so we need to store the school establishment ID in the claims.
            signIn.EstablishmentId = satEstablishment.EstablishmentId;
            await currentUser.SetFocusedSchoolAsync(satEstablishment.Urn, satEstablishment.Name);
        }

        await currentUser.SetFocusedSchoolAsync(dsiOrganisation.Reference, dsiOrganisation.Name);
    }

    private static async Task ProcessGroupLogin(
        ILogger<IDfeSignIn> logger,
        UserInformationReceivedContext context,
        ICurrentUserProvider currentUser,
        EstablishmentModel dsiOrganisation,
        string dsiUserReference
    )
    {
        if (string.IsNullOrWhiteSpace(dsiOrganisation.GroupUid))
        {
            logger.LogWarning(
                "No group UID found in DSI data for group {OrgName} when user with nameidentifier {DsiReference} signed in",
                dsiOrganisation.Name,
                dsiUserReference
            );
        }

        var giasService = context.HttpContext.RequestServices.GetRequiredService<IGiasService>();
        var groupSchools = await giasService.GetSchoolsForMatAsync(dsiOrganisation);

        await currentUser.SetGroupSchools(groupSchools);
    }

    private static async Task<SelectedEstablishmentDataModel?> GetSingleAcademySchoolEstablishmentAsync(
        UserInformationReceivedContext context,
        EstablishmentModel dsiUserOrganisation
    )
    {
        if (
            dsiUserOrganisation?.Category is null
            || !DsiConstants.SatOrganisationCategoryIds.Contains(dsiUserOrganisation.Category.Id)
        )
        {
            return null;
        }

        var giasService = context.HttpContext.RequestServices.GetRequiredService<IGiasService>();
        var establishment = await giasService.GetSchoolForSatAsync(dsiUserOrganisation);

        if (establishment is not null)
        {
            return establishment;
        }

        return null;
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

        ClaimsIdentity claimsIdentity = new([
            new Claim(ClaimConstants.DB_USER_ID, signin.UserId.ToString()),
            new Claim(ClaimConstants.SessionId, Guid.NewGuid().ToString()),
        ]);

        principal.AddIdentity(claimsIdentity);
    }
}

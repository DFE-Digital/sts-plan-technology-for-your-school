using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services;

public class GiasService(IEstablishmentWorkflow establishmentWorkflow, IGiasWorkflow giasWorkflow)
    : IGiasService
{
    private readonly IEstablishmentWorkflow _establishmentWorkflow =
        establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly IGiasWorkflow _giasWorkflow =
        giasWorkflow ?? throw new ArgumentNullException(nameof(giasWorkflow));

    public async Task<int?> GetSchoolEstablishmentIdForSatAsync(EstablishmentModel dsiOrganisation)
    {
        var categoryId = dsiOrganisation.Category?.Id;

        if (categoryId is null || !DsiConstants.SatOrganisationCategoryIds.Contains(categoryId))
        {
            return null;
        }

        // Now we know the user is from a (S)SAT, find the user's corresponding school.
        SqlGiasEstablishmentDto? school = null;
        if (int.TryParse(dsiOrganisation.Uid, out var satGroupUid))
        {
            school = await _giasWorkflow.GetSingleAcademySchool(satGroupUid);
        }

        // Throw if not found
        if (school is null)
        {
            var orgType = categoryId == DsiConstants.SatOrganisationCategoryId ? "SAT" : "SSAT";

            throw new InvalidGiasDataException(
                $"School not found for {orgType} with group UID '{dsiOrganisation.Uid}'"
            );
        }

        var establishmentModel = new EstablishmentModel
        {
            Ukprn = school.Ukprn,
            Urn = school.Urn.ToString(),
            Type = school.TypeOfEstablishmentName is null
                ? null
                : new IdWithNameModel { Name = school.TypeOfEstablishmentName },
            Name = school.EstablishmentName,
            GroupUid = dsiOrganisation.GroupUid,
        };

        var establishment = await _establishmentWorkflow.GetOrCreateEstablishmentAsync(
            establishmentModel
        );

        return establishment.Id;
    }
}

using System.Globalization;
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

    public Task<bool> HasSchoolInGroup(string groupUid, string schoolUrn)
    {
        return HasAllSchoolsInGroup(groupUid, [schoolUrn]);
    }

    public Task<bool> HasAllSchoolsInGroup(string groupUid, IEnumerable<string> schoolUrns)
    {
        if (!int.TryParse(groupUid, out var uid))
        {
            throw new InvalidDataException("Cannot parse an integer UID from the provided string");
        }

        var urns = schoolUrns
            .Select(s =>
                int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out var urn)
                    ? urn
                    : throw new FormatException($"Cannot parse an integer URN from '{s}'")
            )
            .ToList();

        return _giasWorkflow.HasAllSchoolsInGroup(uid, urns);
    }

    public async Task<SelectedEstablishmentDataModel?> GetSchoolForSatAsync(
        EstablishmentModel dsiOrganisation
    )
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
                $"School not found for {orgType} with group UID '{dsiOrganisation.GroupUid}'"
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

        var dbEstablishment = await _establishmentWorkflow.GetOrCreateEstablishmentAsync(
            establishmentModel
        );

        return new SelectedEstablishmentDataModel
        {
            Id = dbEstablishment.Id,
            Name = school.EstablishmentName,
            Urn = school.Urn.ToString(),
        };
    }

    public async Task<List<SelectedEstablishmentDataModel>> GetSchoolsForMatAsync(
        EstablishmentModel dsiOrganisation
    )
    {
        var categoryId = dsiOrganisation.Category?.Id;
        if (
            categoryId is null
            || !DsiConstants.SelectASchoolOrganisationCategoryIds.Contains(categoryId)
        )
        {
            return [];
        }

        // Now we know the user is from a group that can select schools, find the group's corresponding schools.
        IEnumerable<SqlGiasEstablishmentDto>? schools = null;
        if (int.TryParse(dsiOrganisation.Uid, out var groupUid))
        {
            schools = await _giasWorkflow.GetSchoolsInGroup(groupUid);
        }

        if (schools is null)
        {
            throw new InvalidGiasDataException(
                $"No schools found for group UID '{dsiOrganisation.GroupUid}'"
            );
        }

        var groupSchools = new List<SqlGiasEstablishmentDto>();
        var establishmentModels = schools.Select(school => new EstablishmentModel
        {
            Ukprn = school.Ukprn,
            Urn = school.Urn.ToString(),
            Type = school.TypeOfEstablishmentName is null
                ? null
                : new IdWithNameModel { Name = school.TypeOfEstablishmentName },
            Name = school.EstablishmentName,
            GroupUid = dsiOrganisation.GroupUid,
        });

        var dbEstablishments = await _establishmentWorkflow.GetOrCreateEstablishmentsAsync(
            establishmentModels
        );

        return dbEstablishments
            .Select(dbEstablishment => new SelectedEstablishmentDataModel
            {
                Id = dbEstablishment.Id,
                Name = dbEstablishment.OrgName!,
                Urn = dbEstablishment.EstablishmentRef,
            })
            .ToList();
    }
}

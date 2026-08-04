using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class SignInWorkflow(
    IEstablishmentRepository establishmentRepository,
    IGiasRepository giasRepository,
    ISignInRepository signInRepository,
    IUserRepository userRepository
) : ISignInWorkflow
{
    private readonly IEstablishmentRepository _establishmentRepository =
        establishmentRepository ?? throw new ArgumentNullException(nameof(establishmentRepository));
    private readonly IGiasRepository _giasRepository =
        giasRepository ?? throw new ArgumentNullException(nameof(giasRepository));
    private readonly ISignInRepository _signInRepository =
        signInRepository ?? throw new ArgumentNullException(nameof(signInRepository));
    private readonly IUserRepository _userRepository =
        userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    public virtual async Task<(
        EstablishmentModel updatedOrganisation,
        SqlSignInDto signIn
    )> RecordSignIn(string dfeSignInRef, EstablishmentModel dsiOrganisation)
    {
        var user = await GetOrCreateUserAsync(dfeSignInRef);

        // If the user is a (S)SAT, ensure we treat the user as though they're a school.
        if (
            dsiOrganisation.Category != null
            && DsiConstants.SatOrganisationCategoryIds.Contains(dsiOrganisation.Category.Id)
        )
        {
            GiasEstablishmentEntity? school = null;
            if (int.TryParse(dsiOrganisation.Uid, out var satGroupUid))
            {
                school = await _giasRepository.GetSingleAcademySchool(satGroupUid);
            }

            var organisationType = dsiOrganisation.Category.Id switch
            {
                DsiConstants.SatOrganisationCategoryId => "SAT",
                DsiConstants.SSatOrganisationCategoryId => "SSAT",
                _ => "unknown organisation type",
            };

            if (school is null)
            {
                throw new InvalidOperationException(
                    $"GIAS establishment not found for {organisationType} with UID '{dsiOrganisation.Uid}'"
                );
            }

            dsiOrganisation = new EstablishmentModel
            {
                Id = Guid.NewGuid(),
                Ukprn = school.Ukprn,
                Urn = school.Urn.ToString(),
                Type = new IdWithNameModel
                {
                    Name = school.TypeOfEstablishment?.TypeOfEstablishmentName ?? string.Empty,
                },
                Name = school.EstablishmentName,
                GroupUid = satGroupUid.ToString(),
            };
        }

        var establishment = await GetOrCreateEstablishmentAsync(dsiOrganisation);
        var signIn = await _signInRepository.CreateSignInAsync(user.Id, establishment.Id);

        return (dsiOrganisation, signIn.AsDto());
    }

    public async Task<SqlSignInDto> RecordSignInUserOnly(string dfeSignInRef)
    {
        var user = await GetOrCreateUserAsync(dfeSignInRef);
        var signIn = await _signInRepository.CreateSignInAsync(user.Id);

        return signIn.AsDto();
    }

    private async Task<SqlUserDto> GetOrCreateUserAsync(string dfeSignInRef)
    {
        var existingUser = await _userRepository.GetUserBySignInRefAsync(dfeSignInRef);
        if (existingUser is not null)
        {
            return existingUser.AsDto();
        }

        var newUser = await _userRepository.CreateUserBySignInRefAsync(dfeSignInRef);
        return newUser.AsDto();
    }

    private async Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(
        EstablishmentModel establishmentModel
    )
    {
        var existingEstablishment = await _establishmentRepository.GetEstablishmentByReferenceAsync(
            establishmentModel.Reference
        );
        if (existingEstablishment is not null)
        {
            return existingEstablishment.AsDto();
        }

        var newEstablishmentData = new EstablishmentModel
        {
            Ukprn = establishmentModel.Ukprn,
            Urn = establishmentModel.Urn,
            Type = establishmentModel.Type?.Name is null
                ? null
                : new IdWithNameModel { Name = establishmentModel.Type.Name },
            Name = establishmentModel.Name,
            GroupUid = establishmentModel.Uid,
        };

        var newEstablishment = await _establishmentRepository.CreateEstablishmentFromModelAsync(
            newEstablishmentData
        );

        return newEstablishment.AsDto();
    }
}

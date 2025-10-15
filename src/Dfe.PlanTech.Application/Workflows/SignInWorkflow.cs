using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class SignInWorkflow(
    IEstablishmentRepository establishmentRepository,
    ISignInRepository signInRepository,
    IUserRepository userRepository
) : ISignInWorkflow
{
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository ?? throw new ArgumentNullException(nameof(establishmentRepository));
    private readonly ISignInRepository _signInRepository = signInRepository ?? throw new ArgumentNullException(nameof(signInRepository));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    public virtual async Task<SqlSignInDto> RecordSignIn(string dfeSignInRef, DsiOrganisationModel? dsiOrganisationModel)
    {
        var user = await GetOrCreateUserAsync(dfeSignInRef);

        if (dsiOrganisationModel is null)
        {
            var signIn = await _signInRepository.CreateSignInAsync(user.Id, null);
            return signIn.AsDto();
        }
        else
        {
            var establishment = await GetOrCreateEstablishmentAsync(dsiOrganisationModel); // TODO/FIXME: Not necessarily an establishment
            var signIn = await _signInRepository.CreateSignInAsync(user.Id, establishment.Id);
            return signIn.AsDto();
        }
    }

    public async Task<SqlSignInDto> RecordSignInUserOnly(string dfeSignInRef)
    {
        return await RecordSignIn(dfeSignInRef, null);
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

    private async Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(DsiOrganisationModel dsiOrganisationModel)
    {
        var existingEstablishment = await _establishmentRepository.GetEstablishmentByReferenceAsync(dsiOrganisationModel.Reference);
        if (existingEstablishment is not null)
        {
            return existingEstablishment.AsDto();
        }

        var newEstablishmentData = new DsiOrganisationModel
        {
            Ukprn = dsiOrganisationModel.Ukprn,
            Urn = dsiOrganisationModel.Urn,
            Type = dsiOrganisationModel.Type?.Name is null
                ? null
                : new IdWithNameModel { Name = dsiOrganisationModel.Type.Name },
            Name = dsiOrganisationModel.Name,
            GroupUid = dsiOrganisationModel.Uid
        };

        var newEstablishment = await _establishmentRepository
            .CreateEstablishmentFromModelAsync(newEstablishmentData);

        return newEstablishment.AsDto();
    }
}

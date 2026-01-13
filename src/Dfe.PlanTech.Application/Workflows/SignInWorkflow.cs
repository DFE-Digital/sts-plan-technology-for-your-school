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

    public virtual async Task<SqlSignInDto> RecordSignIn(string dfeSignInRef, EstablishmentModel establishmentModel)
    {
        var user = await GetOrCreateUserAsync(dfeSignInRef);
        var establishment = await GetOrCreateEstablishmentAsync(establishmentModel);
        var signIn = await _signInRepository.CreateSignInAsync(user.Id, establishment.Id);

        return signIn.AsDto();
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

    private async Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(EstablishmentModel establishmentModel)
    {
        var existingEstablishment = await _establishmentRepository.GetEstablishmentByReferenceAsync(establishmentModel.Reference);
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
            GroupUid = establishmentModel.Uid
        };

        var newEstablishment = await _establishmentRepository
            .CreateEstablishmentFromModelAsync(newEstablishmentData);

        return newEstablishment.AsDto();
    }
}

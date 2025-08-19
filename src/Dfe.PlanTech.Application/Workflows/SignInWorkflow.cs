using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Application.Workflows
{
    public class SignInWorkflow
    {
        private readonly EstablishmentRepository _establishmentRepository;
        private readonly SignInRepository _signInRepository;
        private readonly UserRepository _userRepository;

        public SignInWorkflow(
            EstablishmentRepository establishmentRepository,
            SignInRepository signInRepository,
            UserRepository userRepository
        )
        {
            _establishmentRepository = establishmentRepository;
            _signInRepository = signInRepository;
            _userRepository = userRepository;
        }

        public async Task<SqlSignInDto> RecordSignIn(string dfeSignInRef, EstablishmentModel establishmentModel)
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
                    : new EstablishmentTypeModel { Name = establishmentModel.Type.Name },
                Name = establishmentModel.Name,
                GroupUid = establishmentModel.Uid
            };

            var newEstablishment = await _establishmentRepository
                .CreateEstablishmentFromModelAsync(newEstablishmentData);

            return newEstablishment.AsDto();
        }
    }
}

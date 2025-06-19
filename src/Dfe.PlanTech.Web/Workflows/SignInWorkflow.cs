using Dfe.PlanTech.Domain.DataTransferObjects;
using Dfe.PlanTech.Domain.Models;
using Dfe.PlanTech.Infrastructure.Data.Repositories;

namespace Dfe.PlanTech.Web.Workflows
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

        public async Task<SignInDto> RecordSignIn(RecordUserSignInDto recordUserSignInDto)
        {
            var user = await UpsertUserAsync(recordUserSignInDto.DfeSignInRef);
            var establishment = await UpsertEstablishmentAsync(recordUserSignInDto);
            var signIn = await _signInRepository.CreateSignInAsync(user.Id, establishment.Id);

            return signIn.ToDto();
        }

        private async Task<UserDto> UpsertUserAsync(string dfeSignInRef)
        {
            var existingUser = await _userRepository.GetUserBySignInRefAsync(dfeSignInRef);
            if (existingUser is not null)
            {
                return existingUser.ToDto();
            }

            var newUser = await _userRepository.CreateUserBySignInRefAsync(dfeSignInRef);
            return newUser.ToDto();
        }

        private async Task<EstablishmentDto> UpsertEstablishmentAsync(RecordUserSignInDto recordUserSignInDto)
        {
            var existingEstablishment = await _establishmentRepository.GetEstablishmentIdFromRefAsync(recordUserSignInDto.Organisation.Reference);
            if (existingEstablishment is not null)
            {
                return existingEstablishment.ToDto();
            }

            var newEstablishment = await _establishmentRepository.CreateEstablishmentFromModelAsync(new EstablishmentModel
            {
                Ukprn = recordUserSignInDto.Organisation.Ukprn,
                Urn = recordUserSignInDto.Organisation.Urn,
                Type = recordUserSignInDto.Organisation.Type?.Name == null ? null : new EstablishmentTypeModel()
                {
                    Name = recordUserSignInDto.Organisation.Type.Name
                },
                OrgName = recordUserSignInDto.Organisation.Name,
                GroupUid = recordUserSignInDto.Organisation.Uid
            });

            return newEstablishment.ToDto();
        }
    }
}

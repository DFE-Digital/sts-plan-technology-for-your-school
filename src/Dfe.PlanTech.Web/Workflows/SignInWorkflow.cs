using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Domain.Models;
using Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

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

        public async Task<SqlSignInDto> RecordSignIn(string dfeSignInRef, string organisationReference)
        {
            var user = await UpsertUserAsync(dfeSignInRef);
            var establishment = await UpsertEstablishmentAsync(organisationReference);
            var signIn = await _signInRepository.CreateSignInAsync(establishment.Id, user.Id);

            return signIn.ToDto();
        }

        private async Task<SqlUserDto> UpsertUserAsync(string dfeSignInRef)
        {
            var existingUser = await _userRepository.GetUserBySignInRefAsync(dfeSignInRef);
            if (existingUser is not null)
            {
                return existingUser.ToDto();
            }

            var newUser = await _userRepository.CreateUserBySignInRefAsync(dfeSignInRef);
            return newUser.ToDto();
        }

        private async Task<SqlEstablishmentDto> UpsertEstablishmentAsync(string organisationReference)
        {
            var existingEstablishment = await _establishmentRepository.GetEstablishmentIdFromRefAsync(organisationReference);
            if (existingEstablishment is not null)
            {
                return existingEstablishment.ToDto();
            }

            var newEstablishmentData = new EstablishmentModel
            {
                Ukprn = recordUserSignInDto.Organisation.Ukprn,
                Urn = recordUserSignInDto.Organisation.Urn,
                Type = recordUserSignInDto.Organisation.Type?.Name == null ? null : new EstablishmentTypeModel()
                {
                    Name = recordUserSignInDto.Organisation.Type.Name
                },
                OrgName = recordUserSignInDto.Organisation.Name,
                GroupUid = recordUserSignInDto.Organisation.Uid
            };

            var newEstablishment = await _establishmentRepository
                .CreateEstablishmentFromModelAsync(newEstablishmentData);

            return newEstablishment.ToDto();
        }
    }
}

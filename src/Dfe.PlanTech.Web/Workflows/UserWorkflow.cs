using System.Text.Json;
using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.DataTransferObjects;
using Dfe.PlanTech.Domain.Models;
using Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

namespace Dfe.PlanTech.Web.Workflows
{
    public class UserWorkflow
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly EstablishmentRepository _establishmentRepository;
        private readonly UserRepository _userRepository;

        public UserWorkflow(
            IHttpContextAccessor httpContextAccessor,
            EstablishmentRepository establishmentRepository,
            UserRepository userRepository
        )
        {
            _contextAccessor = httpContextAccessor;
            _establishmentRepository = establishmentRepository;
            _userRepository = userRepository;
        }

        public async Task<UserDto?> GetCurrentUserAsync()
        {
            var dfeSignInRef = GetStringFromClaim(ClaimConstants.NameIdentifier);
            if (dfeSignInRef is null)
            {
                return null;
            }

            var user = await _userRepository.GetUserBySignInRefAsync(dfeSignInRef);
            return user?.ToDto();
        }

        public async Task<int?> GetCurrentUserIdAsync()
        {
            var dbUserId = GetIntFromClaim(ClaimConstants.DB_USER_ID);
            if (dbUserId is not null)
            {
                return null;
            }

            var user = await GetCurrentUserAsync();
            return user?.Id;
        }

        public async Task<int> GetCurrentUserEstablishmentIdAsync()
        {
            var dbEstablishmentId = GetIntFromClaim(ClaimConstants.DB_ESTABLISHMENT_ID);
            if (dbEstablishmentId is not null)
            {
                return dbEstablishmentId.Value;
            }

            var establishmentModel = GetEstablishmentModel();
            var establishment = await UpsertEstablishment(establishmentModel);

            return Convert.ToInt16(establishment.Id);
        }

        public async Task<EstablishmentDto> UpsertEstablishment(EstablishmentModel establishmentModel)
        {
            var establishment = await _establishmentRepository.GetEstablishmentIdFromRefAsync(establishmentModel.Reference);
            if (establishment is null)
            {
                var newEstablishment = await _establishmentRepository.CreateEstablishmentFromModelAsync(establishmentModel);
                return newEstablishment.ToDto();
            }

            return establishment.ToDto();
        }

        private EstablishmentModel GetEstablishmentModel()
        {
            var organisationDetails = GetStringFromClaim(ClaimConstants.Organisation);
            if (organisationDetails is null)
            {
                throw new KeyNotFoundException($"Could not find {ClaimConstants.Organisation} claim type");
            }

            var establishment = JsonSerializer.Deserialize<EstablishmentModel>(organisationDetails);
            if (establishment is null || !establishment.IsValid)
                throw new InvalidDataException("Establishment was not in expected format");

            return establishment;
        }

        private int? GetIntFromClaim(string type)
        {
            var id = _contextAccessor
                .HttpContext?
                .User
                .Claims
                .FirstOrDefault(claim => claim.Type == type)?
                .Value;

            return id != null
                ? int.Parse(id)
                : null;
        }

        private int? GetNameIdentifierFromClaim(string claimType)
        {
            var value = _contextAccessor
                .HttpContext?
                .User
                .Claims
                .FirstOrDefault(claim => claim.Type == claimType)?
                .Value;

            return value != null
                ? int.Parse(value)
                : null;
        }

        private string? GetStringFromClaim(string claimType)
        {
            return _contextAccessor
                .HttpContext?
                .User
                .Claims
                .FirstOrDefault(x => x.Type.Contains(claimType))?
                .Value;
        }
    }
}


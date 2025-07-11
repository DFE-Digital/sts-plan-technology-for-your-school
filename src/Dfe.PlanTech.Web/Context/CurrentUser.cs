using System.Text.Json;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Web.Context
{
    public class CurrentUser(IHttpContextAccessor contextAccessor)
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

        public string? DsiRef => GetStringFromClaim(ClaimConstants.NameIdentifier);
        public string? Email => GetNameIdentifierFromClaim(ClaimConstants.VerifiedEmail);
        public int? EstablishmentId => GetIntFromClaim(ClaimConstants.DB_ESTABLISHMENT_ID);
        public int? UserId => GetIntFromClaim(ClaimConstants.DB_USER_ID);

        public bool IsInRole(string role) => contextAccessor.HttpContext?.User.IsInRole(role) ?? false;

        public EstablishmentModel GetEstablishmentModel()
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

        private int? GetIntFromClaim(string claimType)
        {
            return int.TryParse(GetNameIdentifierFromClaim(claimType), out var id) ? id : null;
        }

        private string? GetNameIdentifierFromClaim(string claimType)
        {
            return _contextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
        }

        private string? GetStringFromClaim(string claimType)
        {
            return _contextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Contains(claimType))?.Value;
        }
    }
}

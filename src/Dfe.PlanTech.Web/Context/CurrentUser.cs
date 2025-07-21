using System.Security.Authentication;
using System.Text.Json;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Web.Context
{
    public class CurrentUser(IHttpContextAccessor contextAccessor)
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

        private const string MatOrganisationCategoryId = "010";

        private OrganisationModel? _organisationModel;

        public string? DsiReference => GetStringFromClaim(ClaimConstants.NameIdentifier)
            ?? throw new AuthenticationException("User is not authenticated");
        public string? Email => GetNameIdentifierFromClaim(ClaimConstants.VerifiedEmail)
            ?? throw new AuthenticationException($"User's {nameof(Email)} is null");
        public int? EstablishmentId => GetIntFromClaim(ClaimConstants.DB_ESTABLISHMENT_ID);
        public bool IsAuthenticated => GetIsAuthenticated();
        public bool IsMat => Organisation?.Category?.Id.Equals(MatOrganisationCategoryId) ?? false;
        public OrganisationModel? Organisation
        {
            get
            {
                if (_organisationModel is null)
                {
                    ParseOrganisationModel();
                }

                return _organisationModel;
            }
        }
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

        private bool GetIsAuthenticated()
        {
            return _contextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        }

        private string? GetNameIdentifierFromClaim(string claimType)
        {
            return _contextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
        }

        private string? GetStringFromClaim(string claimType)
        {
            return _contextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type.Contains(claimType))?.Value;
        }

        private void ParseOrganisationModel()
        {
            var organisationClaim = GetNameIdentifierFromClaim(ClaimConstants.Organisation);
            if (string.IsNullOrWhiteSpace(organisationClaim))
            {
                throw new AuthenticationException($"User's {nameof(Organisation)} is null or empty");
            }

            _organisationModel = JsonSerializer.Deserialize<OrganisationModel>(organisationClaim);
            if (_organisationModel is null)
            {
                throw new InvalidDataException($"Could not parse user's {nameof(Organisation)} claim");
            }
        }
    }
}

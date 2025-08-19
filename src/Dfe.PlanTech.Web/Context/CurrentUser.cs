using System.Security.Authentication;
using System.Text.Json;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Infrastructure.SignIns.Extensions;

namespace Dfe.PlanTech.Web.Context
{
    public class CurrentUser(IHttpContextAccessor contextAccessor)
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

        public string? DsiReference => GetStringFromClaim(ClaimConstants.NameIdentifier)
            ?? throw new AuthenticationException("User is not authenticated");

        public string? Email => GetNameIdentifierFromClaim(ClaimConstants.VerifiedEmail)
            ?? throw new AuthenticationException($"User's {nameof(Email)} is null");

        public int? EstablishmentId => GetIntFromClaim(ClaimConstants.DB_MAT_ESTABLISHMENT_ID) ?? GetIntFromClaim(ClaimConstants.DB_ESTABLISHMENT_ID);

        public bool IsAuthenticated => GetIsAuthenticated();

        public bool IsMat => Organisation?.Category?.Id.Equals(DsiConstants.MatOrganisationCategoryId) ?? false;

        public OrganisationModel? Organisation
        {
            get => ParseOrganisationModel();
        }

        public int? UserId => GetIntFromClaim(ClaimConstants.DB_USER_ID);

        public bool IsInRole(string role) => contextAccessor.HttpContext?.User.IsInRole(role) ?? false;

        public void SetMatSelectedSchoolId(string schoolUrn)
        {
            throw new NotImplementedException();
            // Set ClaimConstants.DB_MAT_ESTABLISHMENT_ID to the database's establishment ID for the selected school
        }

        public EstablishmentModel GetEstablishmentModel()
        {
            return _contextAccessor.HttpContext?.User.GetUserOrganisation()
                ?? throw new InvalidDataException("Establishment was not in expected format");
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

        private OrganisationModel? ParseOrganisationModel()
        {
            var organisationClaim = GetNameIdentifierFromClaim(ClaimConstants.Organisation);
            if (string.IsNullOrWhiteSpace(organisationClaim))
            {
                throw new AuthenticationException($"User's {nameof(Organisation)} is null or empty");
            }

            return JsonSerializer.Deserialize<OrganisationModel>(organisationClaim)
                ?? throw new InvalidDataException($"Could not parse user's {nameof(Organisation)} claim");
        }
    }
}

using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Models;
using Dfe.PlanTech.Infrastructure.SignIns.Models;

namespace Dfe.PlanTech.Infrastructure.SignIns.Extensions;

public static class UserClaimsExtensions
{
    private static readonly JsonSerializerOptions _jsonSerialiserOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    public static string GetUserId(this IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        return claims
            .Where(c => c.Type.Contains(ClaimConstants.NameIdentifier))
            .Select(c => c.Value)
            .Single();
    }

    /// <summary>
    /// Gets the organisation of a user by deserializing the organisation claim.
    /// </summary>
    /// <returns>
    /// The deserialized <see cref="Organisation"/>, or a value of <c>null</c> if no
    /// organisation has been provided for the user.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="principal"/> is <c>null</c>
    /// </exception>
    public static Organisation? GetOrganisation(this IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        string? organisationJson = GetUserOrganisationClaim(claims);

        if (organisationJson == null)
        {
            return null;
        }

        var organisation = JsonSerializer.Deserialize<Organisation>(organisationJson, _jsonSerialiserOptions);

        if (organisation?.Id == Guid.Empty)
        {
            return null;
        }

        return organisation;
    }

    public static Organisation? GetUserOrganisation(this ClaimsPrincipal claimsPrincipal) => claimsPrincipal.Claims.GetOrganisation();

    public static UserAuthorisationStatus AuthorisationStatus(this ClaimsPrincipal claimsPrincipal) => new(claimsPrincipal.Identity?.IsAuthenticated == true, claimsPrincipal.GetUserOrganisation() != null);

    private static string? GetUserOrganisationClaim(IEnumerable<Claim> claims) => claims.Where(c => c.Type == ClaimConstants.Organisation).Select(c => c.Value).FirstOrDefault();
}

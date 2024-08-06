using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Dfe.PlanTech.Domain.SignIns.Models;

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

        var organisationJson = claims.Where(c => c.Type == ClaimConstants.Organisation)
            .Select(c => c.Value)
            .FirstOrDefault();

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

}

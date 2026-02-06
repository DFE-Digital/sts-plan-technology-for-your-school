using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.SignIn.Models;

namespace Dfe.PlanTech.Infrastructure.SignIn.Extensions;

[ExcludeFromCodeCoverage]
public static class UserClaimsExtensions
{
    private static readonly JsonSerializerOptions _jsonSerialiserOptions =
        new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

    public static UserAuthorisationStatus GetAuthorisationStatus(
        this ClaimsPrincipal claimsPrincipal
    ) =>
        new(
            claimsPrincipal.Identity?.IsAuthenticated == true,
            claimsPrincipal.Claims.GetOrganisation() != null
        );

    public static string GetDsiReference(this IEnumerable<Claim> claims)
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
    public static EstablishmentModel? GetOrganisation(this IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        string? organisationJson = claims
            .FirstOrDefault(c => c.Type == ClaimConstants.Organisation)
            ?.Value;

        if (organisationJson == null)
        {
            return null;
        }

        var organisation = JsonSerializer.Deserialize<EstablishmentModel>(
            organisationJson,
            _jsonSerialiserOptions
        );
        if (organisation?.Id == Guid.Empty)
        {
            return null;
        }

        return organisation;
    }
}

using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Domain.SignIn.Enums;
using Dfe.PlanTech.Domain.SignIn.Models;

namespace Dfe.PlanTech.Infrastructure.SignIn.Extensions;

public static class UserClaimsExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        return principal.Claims
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
    public static Organisation? GetOrganisation(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        var organisationJson = principal.Claims.Where(c => c.Type == ClaimConstants.Organisation)
            .Select(c => c.Value)
            .FirstOrDefault();

        if (organisationJson == null)
        {
            return null;
        }

        var organisation = JsonSerializer.Deserialize<Organisation>(organisationJson);

        if (organisation?.Id == Guid.Empty)
        {
            return null;
        }

        return organisation;
    }

}

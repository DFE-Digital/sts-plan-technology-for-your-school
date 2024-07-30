using Dfe.PlanTech.Domain.SignIns.Models;

namespace Dfe.PlanTech.Application.SignIns.Interfaces;

/// <summary>
/// A service that interacts with the DfE Sign-in public API.
/// </summary>
/// <seealso cref="https://github.com/DFE-Digital/login.dfe.public-api"/>
public interface IDfePublicApi
{
    /// <summary>
    /// Get user access information for the service. 
    /// </summary>
    /// <seealso href="https://github.com/DFE-Digital/login.dfe.public-api#get-user-access-to-service">Get user access to service</seealso>
    /// <param name="userId">The DfE Sign-in identifier for the user.</param>
    /// <param name="organisationId">The DfE Sign-in identifier for the organisation.</param>
    /// <returns>
    /// An object representing the user's access to the service; or a value of <c>null</c>
    /// if the user is not enrolled into the service.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// If <paramref name="userId"/> or <paramref name="organisationId"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="DfePublicApiException">
    /// If API does not return a successful status code.
    /// </exception>
    Task<UserAccessToService?> GetUserAccessToService(string userId, string organisationId);
}

using Dfe.PlanTech.Domain.Establishments.Models;

namespace Dfe.PlanTech.Domain.Users.Interfaces;

public interface IUser
{
    Task<int?> GetCurrentUserId();

    Task<int> GetEstablishmentId();

    Task<string?> GetEstablishmentGroupName();

    EstablishmentDto GetOrganisationData();
}

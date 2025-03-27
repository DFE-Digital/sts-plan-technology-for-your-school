using Dfe.PlanTech.Domain.Establishments.Models;

namespace Dfe.PlanTech.Domain.Users.Interfaces;

public interface IUser
{
    Task<int?> GetCurrentUserId();

    Task<int> GetEstablishmentId();

    Task<List<EstablishmentLink>> GetGroupEstablishments();

    EstablishmentDto GetOrganisationData();
}

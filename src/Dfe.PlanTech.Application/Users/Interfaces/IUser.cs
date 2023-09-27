using Dfe.PlanTech.Domain.Establishments.Models;

namespace Dfe.PlanTech.Application.Users.Interfaces
{
    public interface IUser
    {
        Task<int?> GetCurrentUserId();

        Task<int> GetEstablishmentId();

        EstablishmentDto GetOrganisationData();
    }
}

using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Providers.Interfaces
{
    public interface IDsiOrganisationProvider
    {
        Task<EstablishmentModel?> GetOrganisationForUserAsync(string userDsiReference, string urn);
    }
}

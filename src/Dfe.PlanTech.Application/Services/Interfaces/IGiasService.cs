using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IGiasService
{
    Task<int?> GetSchoolEstablishmentIdForSatAsync(EstablishmentModel dsiOrganisation);
}

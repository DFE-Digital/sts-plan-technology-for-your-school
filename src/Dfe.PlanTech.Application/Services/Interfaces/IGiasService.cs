using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IGiasService
{
    Task<bool> HasSchoolInGroup(string groupUid, string schoolUrn);
    Task<bool> HasAllSchoolsInGroup(string userGroupUid, IEnumerable<string> schoolUrns);
    Task<SelectedEstablishmentDataModel?> GetSchoolForSatAsync(EstablishmentModel dsiOrganisation);
    Task<List<SelectedEstablishmentDataModel>> GetSchoolsForMatAsync(
        EstablishmentModel dsiOrganisation
    );
}

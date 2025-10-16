using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IEstablishmentWorkflow
{
    Task<SqlEstablishmentDto?> GetEstablishmentByDsiReferenceAsync(string establishmentDsiReference);
    Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByDsiReferencesAsync(IEnumerable<string> establishmentDsiReferences);
    Task<List<SqlEstablishmentLinkDto>> GetGroupEstablishments(int establishmentId);
    Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(DsiOrganisationModel dsiOrganisationModel);
    Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(string establishmentUrn, string establishmentName);
    Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel);
}

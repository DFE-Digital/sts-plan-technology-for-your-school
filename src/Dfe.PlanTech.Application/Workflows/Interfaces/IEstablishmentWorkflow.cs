using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IEstablishmentWorkflow
{
    Task<SqlEstablishmentDto?> GetEstablishmentByIdAsync(int id);
    Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(string establishmentReference);
    Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByReferencesAsync(
        IEnumerable<string> establishmentReferences
    );
    Task<List<SqlEstablishmentLinkDto>> GetGroupEstablishments(int establishmentId);
    Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(EstablishmentModel establishmentModel);
    Task<SqlEstablishmentDto> GetOrCreateSchoolAsync(
        string establishmentUrn,
        string establishmentName
    );
    Task<List<SqlEstablishmentDto>> GetOrCreateEstablishmentsAsync(
        IEnumerable<EstablishmentModel> establishmentModels
    );
    Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel);
}

using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IEstablishmentService
{
    Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(EstablishmentModel establishmentModel);

    Task<SqlEstablishmentDto> GetOrCreateSchoolAsync(
        string establishmentUrn,
        string establishmentName
    );

    Task<List<SqlEstablishmentDto>> GetOrCreateSchoolsAsync(
        IEnumerable<SelectedEstablishmentDataModel> schoolsData
    );

    Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByReferencesAsync(
        IEnumerable<string> establishmentReferences
    );

    Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(string establishmentReference);

    Task<SqlEstablishmentDto> GetEstablishmentByIdAsync(int id);

    Task RecordGroupSelection(
        int userId,
        int userEstablishmentId,
        int selectedEstablishmentId,
        string selectedEstablishmentUrn,
        string selectedEstablishmentName
    );
}

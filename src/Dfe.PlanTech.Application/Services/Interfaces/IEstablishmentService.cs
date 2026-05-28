using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IEstablishmentService
{
    Task<List<SqlEstablishmentLinkDto>> GetEstablishmentLinksWithRecommendationCounts(
        int establishmentId
    );
    Task<List<SqlEstablishmentLinkDto>> GetEstablishmentLinks(
        int establishmentId
    );
    Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByReferencesAsync(
        IEnumerable<string> establishmentReferences
    );

    Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(string establishmentReference);
    Task RecordGroupSelection(
        string userDsiReference,
        int? userEstablishmentId,
        EstablishmentModel userEstablishmentModel,
        string selectedEstablishmentUrn,
        string selectedEstablishmentName
    );
}

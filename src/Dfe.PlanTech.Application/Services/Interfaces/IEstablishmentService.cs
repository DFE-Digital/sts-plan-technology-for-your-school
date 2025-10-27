using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IEstablishmentService
{
    Task<List<SqlEstablishmentLinkDto>> GetEstablishmentLinksWithSubmissionStatusesAndCounts(IEnumerable<QuestionnaireCategoryEntry> categories, int establishmentId);
    Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(string establishmentReference);
    Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(EstablishmentModel establishmentModel);
    Task RecordGroupSelection(string userDsiReference, int? userEstablishmentId, EstablishmentModel userEstablishmentModel, string selectedEstablishmentUrn, string selectedEstablishmentName);
}

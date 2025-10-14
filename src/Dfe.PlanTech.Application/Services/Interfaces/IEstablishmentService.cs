using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IEstablishmentService
{
    Task<List<SqlEstablishmentLinkDto>> GetEstablishmentLinksWithSubmissionStatusesAndCounts(IEnumerable<QuestionnaireCategoryEntry> categories, int establishmentId);
    Task<SqlEstablishmentDto> GetLatestSelectedGroupSchoolAsync(string selectedEstablishmentUrn);
    Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(DsiOrganisationModel dsiOrganisationModel);
    Task RecordGroupSelection(string userDsiReference, int? userEstablishmentId, DsiOrganisationModel userDsiOrganisationModel, string selectedEstablishmentUrn, string selectedEstablishmentName);
}

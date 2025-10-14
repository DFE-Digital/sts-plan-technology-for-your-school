using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Services;

public class EstablishmentService(
    ILogger<EstablishmentService> logger,
    IEstablishmentWorkflow establishmentWorkflow,
    ISubmissionWorkflow submissionWorkflow,
    IUserWorkflow userWorkflow
) : IEstablishmentService
{
    private readonly ILogger<EstablishmentService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IEstablishmentWorkflow _establishmentWorkflow = establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly ISubmissionWorkflow _submissionWorkflow = submissionWorkflow ?? throw new ArgumentNullException(nameof(submissionWorkflow));
    private readonly IUserWorkflow _userWorkflow = userWorkflow ?? throw new ArgumentNullException(nameof(userWorkflow));

    public Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(DsiOrganisationModel dsiOrganisationModel)
    {
        return _establishmentWorkflow.GetOrCreateEstablishmentAsync(dsiOrganisationModel);
    }

    public async Task<SqlEstablishmentDto> GetLatestSelectedGroupSchoolAsync(string selectedEstablishmentUrn)
    {
        return await _establishmentWorkflow.GetEstablishmentByDsiReferenceAsync(selectedEstablishmentUrn)
            ?? throw new DatabaseException($"Could not get latest selected group school for {selectedEstablishmentUrn}");
    }

    public async Task<List<SqlEstablishmentLinkDto>> GetEstablishmentLinksWithSubmissionStatusesAndCounts(IEnumerable<QuestionnaireCategoryEntry> categories, int establishmentId)
    {
        var schools = await _establishmentWorkflow.GetGroupEstablishments(establishmentId);
        var sectionIds = categories.SelectMany(c => c.Sections.Select(s => s.Id));

        var schoolUrns = schools.Select(s => s.Urn);
        var establishments = await _establishmentWorkflow.GetEstablishmentsByDsiReferencesAsync(schoolUrns);
        var establishmentLinkMap = establishments.ToDictionary(e => e.Id, e => schools.Single(s => s.Urn.Equals(e.EstablishmentRef)));

        foreach (var establishment in establishments)
        {
            var sectionStatuses = await _submissionWorkflow.GetSectionStatusesAsync(establishment.Id, sectionIds);
            establishmentLinkMap[establishment.Id].CompletedSectionsCount = sectionStatuses.Count(ss => ss.Completed || ss.LastCompletionDate is not null);
        }

        return establishmentLinkMap.Values.ToList();
    }

    public async Task RecordGroupSelection(
        string userDsiReference,
        int? userEstablishmentId,
        DsiOrganisationModel userDsiOrganisationModel,
        string selectedEstablishmentUrn,
        string selectedEstablishmentName)
    {
        var user = await _userWorkflow.GetUserBySignInRefAsync(userDsiReference)
            ?? throw new InvalidDataException("User does not exist");

        if (userEstablishmentId is null)
        {
            var userEstablishment = await _establishmentWorkflow.GetOrCreateEstablishmentAsync(userDsiOrganisationModel);
            userEstablishmentId = userEstablishment.Id;
        }

        var selectedEstablishment = await _establishmentWorkflow.GetEstablishmentByDsiReferenceAsync(selectedEstablishmentUrn);
        selectedEstablishment ??= await _establishmentWorkflow.GetOrCreateEstablishmentAsync(selectedEstablishmentUrn, selectedEstablishmentName);

        var selectionModel = new UserGroupSelectionModel
        {
            SelectedEstablishmentId = selectedEstablishment.Id,
            SelectedEstablishmentName = selectedEstablishmentName,
            UserEstablishmentId = userEstablishmentId.Value,
            UserId = user.Id
        };

        await _establishmentWorkflow.RecordGroupSelection(selectionModel);
    }
}

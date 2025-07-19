using System.Security.Authentication;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.RoutingDataModel;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Services;

public class EstablishmentService(
    ILogger<EstablishmentService> logger,
    EstablishmentWorkflow establishmentWorkflow,
    SubmissionWorkflow submissionWorkflow,
    UserWorkflow userWorkflow
)
{
    private readonly ILogger<EstablishmentService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly EstablishmentWorkflow _establishmentWorkflow = establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly SubmissionWorkflow _submissionWorkflow = submissionWorkflow ?? throw new ArgumentNullException(nameof(submissionWorkflow));
    private readonly UserWorkflow _userWorkflow = userWorkflow ?? throw new ArgumentNullException(nameof(userWorkflow));

    public Task<List<SqlEstablishmentLinkDto>> GetGroupEstablishments(int establishmentId)
    {
        return _establishmentWorkflow.GetGroupEstablishments(establishmentId);
    }

    public Task<SqlGroupReadActivityDto?> GetLatestSelectedGroupSchool(int? userId, int? establishmentId)
    {
        if (userId is null)
        {
            throw new AuthenticationException("User is not authenticated");
        }

        if (establishmentId is null)
        {
            throw new ArgumentException($"User's {nameof(establishmentId)} cannot be null");
        }

        return _establishmentWorkflow.GetLatestSelectedGroupSchool(userId.Value, establishmentId.Value);
    }
    public async Task<List<SqlEstablishmentLinkDto>> BuildSchoolsWithSubmissionCountsView(IEnumerable<CmsCategoryDto> categories, IEnumerable<SqlEstablishmentLinkDto> schools)
    {
        var sectionIds = categories.SelectMany(c => c.Sections.Select(s => s.Id));

        var schoolUrns = schools.Select(s => s.Urn);
        var establishments = await _establishmentWorkflow.GetEstablishmentsByReferencesAsync(schoolUrns);
        var establishmentLinkMap = establishments.ToDictionary(e => e.Id, e => schools.Single(s => s.Urn.Equals(e.EstablishmentRef)));

        foreach (var establishment in establishments)
        {
            var sectionStatuses = await _submissionWorkflow.GetSectionStatusesAsync(establishment.Id, sectionIds);
            establishmentLinkMap[establishment.Id].CompletedSectionsCount = sectionStatuses.Count(ss => ss.Completed || ss.LastCompletionDate is not null);
        }

        return establishmentLinkMap.Values.ToList();
    }

    public async Task<int> RecordGroupSelection(
        string userDsiReference,
        int? userEstablishmentId,
        EstablishmentModel userEstablishmentModel,
        string selectedEstablishmentUrn,
        string selectedEstablishmentName)
    {
        var user = await _userWorkflow.GetUserBySignInRefAsync(userDsiReference)
            ?? throw new InvalidDataException("User does not exist");

        if (userEstablishmentId is null)
        {
            var userEstablishment = await _establishmentWorkflow.GetOrCreateEstablishmentAsync(userEstablishmentModel);
            userEstablishmentId = userEstablishment.Id;
        }

        var selectedEstablishmentId = await _establishmentWorkflow.GetEstablishmentIdByReferenceAsync(selectedEstablishmentUrn);
        if (selectedEstablishmentId is null)
        {
            var selectedEstablishment = await _establishmentWorkflow.GetOrCreateEstablishmentAsync(selectedEstablishmentUrn, selectedEstablishmentName);
            selectedEstablishmentId = selectedEstablishment.Id;
        }

        var selectionModel = new UserGroupSelectionModel
        {
            SelectedEstablishmentId = selectedEstablishmentId.Value,
            SelectedEstablishmentName = selectedEstablishmentName,
            UserEstablishmentId = userEstablishmentId.Value,
            UserId = user.Id
        };

        var selectionId = await _establishmentWorkflow.RecordGroupSelection(selectionModel);

        return selectionId;
    }
}

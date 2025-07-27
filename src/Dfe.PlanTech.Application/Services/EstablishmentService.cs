using System.Security.Authentication;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
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

    public Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(EstablishmentModel establishmentModel)
    {
        return _establishmentWorkflow.GetOrCreateEstablishmentAsync(establishmentModel);
    }

    public async Task<SqlGroupReadActivityDto> GetLatestSelectedGroupSchoolAsync(int? userId, int? establishmentId)
    {
        if (userId is null)
        {
            throw new AuthenticationException("User is not authenticated");
        }

        if (establishmentId is null)
        {
            throw new ArgumentException($"User's {nameof(establishmentId)} cannot be null");
        }

        return await _establishmentWorkflow.GetLatestSelectedGroupSchool(userId.Value, establishmentId.Value)
            ?? throw new DatabaseException($"Could not get latest selected group school for user with ID {userId.Value} in establishment: {establishmentId.Value}");
    }

    public async Task<List<SqlEstablishmentLinkDto>> GetEstablishmentLinksWithSubmissionStatusesAndCounts(IEnumerable<CmsCategoryDto> categories, int establishmentId)
    {
        var schools = await _establishmentWorkflow.GetGroupEstablishments(establishmentId);
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

        var selectedEstablishment = await _establishmentWorkflow.GetEstablishmentByReferenceAsync(selectedEstablishmentUrn);
        selectedEstablishment ??= await _establishmentWorkflow.GetOrCreateEstablishmentAsync(selectedEstablishmentUrn, selectedEstablishmentName);

        var selectionModel = new UserGroupSelectionModel
        {
            SelectedEstablishmentId = selectedEstablishment.Id,
            SelectedEstablishmentName = selectedEstablishmentName,
            UserEstablishmentId = userEstablishmentId.Value,
            UserId = user.Id
        };

        var selectionId = await _establishmentWorkflow.RecordGroupSelection(selectionModel);

        return selectionId;
    }
}

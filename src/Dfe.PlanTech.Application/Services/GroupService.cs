using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Services;

public class GroupService(
    ILoggerFactory loggerFactory,
    EstablishmentWorkflow establishmentWorkflow,
    UserWorkflow userWorkflow
)
{
    private readonly ILogger<GroupService> _logger = loggerFactory.CreateLogger<GroupService>();
    private readonly EstablishmentWorkflow _establishmentWorkflow = establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly UserWorkflow _userWorkflow = userWorkflow ?? throw new ArgumentNullException(nameof(userWorkflow));

    public async Task<int> RecordGroupSelection(string dsiReference, int? establishmentId, EstablishmentModel establishmentModel, string selectedEstablishmentUrn, string selectedEstablishmentName)
    {
        var user = await _userWorkflow.GetUserBySignInRefAsync(dsiReference)
            ?? throw new InvalidDataException("User does not exist");

        var userEstablishmentId = establishmentId
            ?? (await _establishmentWorkflow.GetOrCreateEstablishmentAsync(establishmentModel)).Id;

        var selectedEstablishment = await _establishmentWorkflow.GetEstablishmentByReferenceAsync(selectedEstablishmentUrn)
            ?? (await _establishmentWorkflow.GetOrCreateEstablishmentAsync(selectedEstablishmentUrn, selectedEstablishmentName));

        var groupSelectionModel = new UserGroupSelectionModel
        {
            SelectedEstablishmentId = selectedEstablishment.Id,
            SelectedEstablishmentName = selectedEstablishmentName,
            UserEstablishmentId = userEstablishmentId,
            UserId = user.Id
        };

        var selectionId = await _establishmentWorkflow.RecordGroupSelection(groupSelectionModel);

        return selectionId;
    }
}

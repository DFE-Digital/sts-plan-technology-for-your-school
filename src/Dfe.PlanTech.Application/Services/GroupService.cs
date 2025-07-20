using System.Security.Authentication;
using Dfe.PlanTech.Application.Workflows;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Services;

public class GroupService(
    ILogger<GroupService> logger,
    EstablishmentWorkflow establishmentWorkflow,
    UserWorkflow userWorkflow
)
{
    private readonly ILogger<GroupService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly EstablishmentWorkflow _establishmentWorkflow = establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly UserWorkflow _userWorkflow = userWorkflow ?? throw new ArgumentNullException(nameof(userWorkflow));

    public async Task<int> RecordGroupSelection(string dsiReference, int? establishmentId, string selectedEstablishmentUrn, string selectedEstablishmentName)
    {
        var user = await _userWorkflow.GetUserBySignInRefAsync(dsiReference)
            ?? throw new InvalidDataException("User does not exist");

        var userEstablishmentId = establishmentId
            ?? (await _establishmentWorkflow.GetOrCreateEstablishmentAsync(_currentUser.GetEstablishmentModel())).Id;

        var selectedEstablishmentId = await _establishmentWorkflow.GetEstablishmentIdByReferenceAsync(selectedEstablishmentUrn)
            ?? (await _establishmentWorkflow.GetOrCreateEstablishmentAsync(selectedEstablishmentUrn, selectedEstablishmentName)).Id;

        var selectionId = await _establishmentWorkflow.RecordGroupSelection(userEstablishmentId, selectedEstablishmentId, selectedEstablishmentName, user.Id);

        return selectionId;
    }
}

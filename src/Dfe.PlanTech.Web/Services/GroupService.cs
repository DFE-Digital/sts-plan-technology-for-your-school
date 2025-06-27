using System.Security.Authentication;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Workflows;

namespace Dfe.PlanTech.Web.Services;

public class GroupService(
    ILogger<GroupService> logger,
    CurrentUser currentUser,
    EstablishmentWorkflow establishmentWorkflow,
    UserWorkflow userWorkflow
)
{
    private readonly ILogger<GroupService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly CurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly EstablishmentWorkflow _establishmentWorkflow = establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly UserWorkflow _userWorkflow = userWorkflow ?? throw new ArgumentNullException(nameof(userWorkflow));

    public async Task<int> RecordGroupSelection(string selectedEstablishmentUrn, string selectedEstablishmentName)
    {
        var userDsiRef = _currentUser.DsiRef ?? throw new AuthenticationException("User is not authenticated");
        var user = await _userWorkflow.GetUserBySignInRefAsync(userDsiRef) ?? throw new InvalidDataException("User does not exist");

        var userEstablishmentId = _currentUser.EstablishmentId
            ?? (await _establishmentWorkflow.GetOrCreateEstablishmentAsync(_currentUser.GetEstablishmentModel())).Id;

        var selectedEstablishmentId = await _establishmentWorkflow.GetEstablishmentIdByReferenceAsync(selectedEstablishmentUrn)
            ?? (await _establishmentWorkflow.GetOrCreateEstablishmentAsync(selectedEstablishmentUrn, selectedEstablishmentName)).Id;

        var selectionId = await _establishmentWorkflow.RecordGroupSelection(userEstablishmentId, selectedEstablishmentId, selectedEstablishmentName, user.Id);

        return selectionId;
    }
}

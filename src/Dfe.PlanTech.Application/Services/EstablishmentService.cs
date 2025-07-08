using System.Security.Authentication;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Application.Context;
using Dfe.PlanTech.Application.Workflows;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Services;

public class EstablishmentService(
    ILogger<EstablishmentService> logger,
    CurrentUser currentUser,
    EstablishmentWorkflow establishmentWorkflow
)
{
    private readonly ILogger<EstablishmentService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly CurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly EstablishmentWorkflow _establishmentWorkflow = establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));

    public Task<SqlGroupReadActivityDto?> GetLatestSelectedGroupSchool()
    {
        if (_currentUser.UserId is null)
        {
            throw new AuthenticationException("User is not authenticated");
        }

        if (_currentUser.EstablishmentId is null)
        {
            throw new ArgumentException($"{nameof(_currentUser.EstablishmentId)} cannot be null");
        }

        return _establishmentWorkflow.GetLatestSelectedGroupSchool(_currentUser.UserId.Value, _currentUser.EstablishmentId.Value);
    }

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

using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services;

public class EstablishmentService(
    IEstablishmentWorkflow establishmentWorkflow,
    IUserWorkflow userWorkflow
) : IEstablishmentService
{
    private readonly IEstablishmentWorkflow _establishmentWorkflow =
        establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly IUserWorkflow _userWorkflow =
        userWorkflow ?? throw new ArgumentNullException(nameof(userWorkflow));

    public Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(
        EstablishmentModel establishmentModel
    )
    {
        return _establishmentWorkflow.GetOrCreateEstablishmentAsync(establishmentModel);
    }

    public Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(
        string establishmentUrn,
        string establishmentName
    )
    {
        return _establishmentWorkflow.GetOrCreateEstablishmentAsync(establishmentUrn, establishmentName);
    }

    public Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByReferencesAsync(IEnumerable<string> establishmentReferences)
    {
        return _establishmentWorkflow.GetEstablishmentsByReferencesAsync(establishmentReferences);
    }

    public async Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(
        string establishmentReference
    )
    {
        return await _establishmentWorkflow.GetEstablishmentByReferenceAsync(
            establishmentReference
        );
    }

    public async Task<SqlEstablishmentDto?> GetEstablishmentByIdAsync(int id)
    {
        return await _establishmentWorkflow.GetEstablishmentByIdAsync(id);
    }

    public async Task RecordGroupSelection(
        string userDsiReference,
        int? userEstablishmentId,
        EstablishmentModel userEstablishmentModel,
        string selectedEstablishmentUrn,
        string selectedEstablishmentName
    )
    {
        var user =
            await _userWorkflow.GetUserBySignInRefAsync(userDsiReference)
            ?? throw new InvalidDataException("User does not exist");

        if (userEstablishmentId is null)
        {
            var userEstablishment = await _establishmentWorkflow.GetOrCreateEstablishmentAsync(
                userEstablishmentModel
            );
            userEstablishmentId = userEstablishment.Id;
        }

        var selectedEstablishment = await _establishmentWorkflow.GetOrCreateEstablishmentAsync(
            selectedEstablishmentUrn,
            selectedEstablishmentName
        );

        var selectionModel = new UserGroupSelectionModel
        {
            SelectedEstablishmentId = selectedEstablishment.Id,
            SelectedEstablishmentName = selectedEstablishmentName,
            UserEstablishmentId = userEstablishmentId.Value,
            UserId = user.Id,
        };

        await _establishmentWorkflow.RecordGroupSelection(selectionModel);
    }
}

using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services;

public class EstablishmentService(
    IEstablishmentWorkflow establishmentWorkflow,
    IRecommendationWorkflow recommendationWorkflow,
    IUserWorkflow userWorkflow
) : IEstablishmentService
{
    private readonly IEstablishmentWorkflow _establishmentWorkflow = establishmentWorkflow
        ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly IRecommendationWorkflow _recommendationWorkflow = recommendationWorkflow
        ?? throw new ArgumentNullException(nameof(recommendationWorkflow));
    private readonly IUserWorkflow _userWorkflow = userWorkflow
        ?? throw new ArgumentNullException(nameof(userWorkflow));

    public Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(
        EstablishmentModel establishmentModel
    )
    {
        return _establishmentWorkflow.GetOrCreateEstablishmentAsync(establishmentModel);
    }

    public Task<SqlEstablishmentDto> GetOrCreateSchoolAsync(
        string establishmentUrn,
        string establishmentName
    )
    {
        return _establishmentWorkflow.GetOrCreateSchoolAsync(establishmentUrn, establishmentName);
    }

    public async Task<List<SqlEstablishmentDto>> GetOrCreateSchoolsAsync(
        IEnumerable<SelectedEstablishmentDataModel> schoolsData
    )
    {
        var establishments = new List<SqlEstablishmentDto>();

        foreach (var school in schoolsData)
        {
            var establishment = await _establishmentWorkflow.GetOrCreateSchoolAsync(
                school.Urn,
                school.Name
            );
            establishments.Add(establishment);
        }

        return establishments;
    }

    public Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByReferencesAsync(
        IEnumerable<string> establishmentReferences
    )
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

    public async Task<SqlEstablishmentDto> GetEstablishmentByIdAsync(int id)
    {
        var establishmentEntity = await _establishmentWorkflow.GetEstablishmentByIdAsync(id)
            ?? throw new KeyNotFoundException($"Establishment with ID {id} not found");

        return establishmentEntity;
    }

    public async Task RecordGroupSelection(
        int userId,
        int userEstablishmentId,
        int selectedEstablishmentId,
        string selectedEstablishmentUrn,
        string selectedEstablishmentName
    )
    {
        var selectedEstablishment = await _establishmentWorkflow.GetEstablishmentByIdAsync(
            selectedEstablishmentId
        );
        selectedEstablishment ??= await _establishmentWorkflow.GetOrCreateSchoolAsync(
            selectedEstablishmentUrn,
            selectedEstablishmentName
        );

        var selectionModel = new UserGroupSelectionModel
        {
            SelectedEstablishmentId = selectedEstablishment.Id,
            SelectedEstablishmentName = selectedEstablishmentName,
            UserEstablishmentId = userEstablishmentId,
            UserId = userId,
        };

        await _establishmentWorkflow.RecordGroupSelectionAsync(selectionModel);
    }
}

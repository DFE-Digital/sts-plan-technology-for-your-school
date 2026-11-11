using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class EstablishmentWorkflow(
    IEstablishmentRepository establishmentRepository,
    IEstablishmentLinkRepository establishmentLinkRepository
) : IEstablishmentWorkflow
{
    private readonly IEstablishmentRepository _establishmentRepository =
        establishmentRepository ?? throw new ArgumentNullException(nameof(establishmentRepository));
    private readonly IEstablishmentLinkRepository _establishmentLinkRepository =
        establishmentLinkRepository
        ?? throw new ArgumentNullException(nameof(establishmentLinkRepository));

    public Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(
        string establishmentUrn,
        string establishmentName
    )
    {
        var establishmentModel = new EstablishmentModel()
        {
            Name = establishmentName,
            Urn = establishmentUrn,
        };

        return GetOrCreateEstablishmentAsync(establishmentModel);
    }

    public async Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(
        EstablishmentModel establishmentModel
    )
    {
        var establishment = await _establishmentRepository.GetEstablishmentByReferenceAsync(
            establishmentModel.Reference
        );
        establishment ??= await _establishmentRepository.CreateEstablishmentFromModelAsync(
            establishmentModel
        );

        return establishment.AsDto();
    }

    public async Task<List<SqlEstablishmentDto>> GetOrCreateEstablishmentsAsync(
        IEnumerable<EstablishmentModel> establishmentModels
    )
    {
        var allReferences = establishmentModels.Select(e => e.Reference);
        var existingEstablishments =
            await _establishmentRepository.GetEstablishmentsByReferencesAsync(allReferences);

        var existingReferences = existingEstablishments.Select(e => e.EstablishmentRef);
        var missingReferences = allReferences.Except(existingReferences);
        var missingModels = establishmentModels.Where(model =>
            missingReferences.Contains(model.Reference)
        );

        var newEstablishments = await _establishmentRepository.CreateEstablishmentsFromModelsAsync(
            missingModels
        );

        var allEstablishments = existingEstablishments.Union(newEstablishments);

        return allEstablishments.Select(e => e.AsDto()).ToList();
    }

    public async Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(
        string establishmentReference
    )
    {
        var establishments = await _establishmentRepository.GetEstablishmentsByReferencesAsync([
            establishmentReference,
        ]);
        return establishments.FirstOrDefault()?.AsDto();
    }

    public async Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByReferencesAsync(
        IEnumerable<string> establishmentReferences
    )
    {
        var establishments = await _establishmentRepository.GetEstablishmentsByReferencesAsync(
            establishmentReferences
        );
        return establishments.Select(e => e.AsDto());
    }

    public async Task<List<SqlEstablishmentLinkDto>> GetGroupEstablishments(int establishmentId)
    {
        var links = await _establishmentLinkRepository.GetGroupEstablishmentsByEstablishmentIdAsync(
            establishmentId
        );
        return links.Select(l => l.AsDto()).ToList();
    }

    public Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel)
    {
        return _establishmentLinkRepository.RecordGroupSelection(userGroupSelectionModel);
    }
}

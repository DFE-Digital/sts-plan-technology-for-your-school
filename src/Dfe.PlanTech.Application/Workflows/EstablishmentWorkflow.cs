using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class EstablishmentWorkflow(
    IEstablishmentRepository establishmentRepository,
    IEstablishmentLinkRepository establishmentLinkRepository,
    IStoredProcedureRepository storedProcedureRepository
) : IEstablishmentWorkflow
{
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository ?? throw new ArgumentNullException(nameof(establishmentRepository));
    private readonly IEstablishmentLinkRepository _establishmentLinkRepository = establishmentLinkRepository ?? throw new ArgumentNullException(nameof(establishmentLinkRepository));
    private readonly IStoredProcedureRepository _storedProcedureRepository = storedProcedureRepository ?? throw new ArgumentNullException(nameof(storedProcedureRepository));

    public async Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(DsiOrganisationModel dsiOrganisationModel)
    {
        var establishment = await _establishmentRepository.GetEstablishmentByReferenceAsync(dsiOrganisationModel.Reference);
        establishment ??= await _establishmentRepository.CreateEstablishmentFromModelAsync(dsiOrganisationModel);

        return establishment.AsDto();
    }

    public Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(string establishmentUrn, string establishmentName)
    {
        var establishmentModel = new DsiOrganisationModel()
        {
            Name = establishmentName,
            Urn = establishmentUrn
        };

        return GetOrCreateEstablishmentAsync(establishmentModel);
    }

    public async Task<SqlEstablishmentDto?> GetEstablishmentByDsiReferenceAsync(string establishmentDsiReference)
    {
        var establishments = await _establishmentRepository.GetEstablishmentsByReferencesAsync([establishmentDsiReference]);
        return establishments.FirstOrDefault()?.AsDto();
    }

    public async Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByDsiReferencesAsync(IEnumerable<string> establishmentDsiReferences)
    {
        var establishments = await _establishmentRepository.GetEstablishmentsByReferencesAsync(establishmentDsiReferences);
        return establishments.Select(e => e.AsDto());
    }

    public async Task<List<SqlEstablishmentLinkDto>> GetGroupEstablishments(int establishmentId)
    {
        var links = await _establishmentLinkRepository.GetGroupEstablishmentsByEstablishmentIdAsync(establishmentId);
        return links.Select(l => l.AsDto()).ToList();
    }

    public Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel)
    {
        return _storedProcedureRepository.RecordGroupSelection(userGroupSelectionModel);
    }
}

using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class EstablishmentWorkflow(
    IEstablishmentRepository establishmentRepository,
    IGiasRepository giasRepository,
    IEstablishmentLinkRepository establishmentLinkRepository
) : IEstablishmentWorkflow
{
    private readonly IEstablishmentRepository _establishmentRepository =
        establishmentRepository ?? throw new ArgumentNullException(nameof(establishmentRepository));
    private readonly IGiasRepository _giasRepository =
    giasRepository ?? throw new ArgumentNullException(nameof(giasRepository));
    private readonly IEstablishmentLinkRepository _establishmentLinkRepository =
        establishmentLinkRepository
        ?? throw new ArgumentNullException(nameof(establishmentLinkRepository));
    public async Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(
        EstablishmentModel establishmentModel
    )
    {
        var establishment = await _establishmentRepository.GetEstablishmentByReferenceAsync(establishmentModel.Reference);
        establishment ??= await _establishmentRepository.CreateEstablishmentFromModelAsync(
            establishmentModel
        );

        return establishment.AsDto();
    }

    public async Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(
        string establishmentUrn,
        string establishmentName
    )
    {
        var establishment = await _establishmentRepository.GetEstablishmentByReferenceAsync(establishmentUrn);
        if (establishment is null)
        {
            var urn = 0;
            var urnOk = int.TryParse(establishmentUrn, out urn);
            GiasEstablishmentEntity? giasEst = null;
            if (urnOk)
            {
                giasEst = giasRepository.GetSchoolEstablishmentByURN(urn).Result;
            }
            var establishmentModel = new EstablishmentModel()
            {
                Name = giasEst?.EstablishmentName ?? establishmentName,
                Urn = establishmentUrn,
                Type = new IdWithNameModel
                {
                    Name = giasEst?.TypeOfEstablishment?.TypeOfEstablishmentName ?? string.Empty
                }
            };
            establishment = await _establishmentRepository.CreateEstablishmentFromModelAsync(establishmentModel);
        }
        return establishment.AsDto();
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

    public async Task<SqlEstablishmentDto?> GetEstablishmentByIdAsync(int id)
    {
        var establishmentEntity = await _establishmentRepository.GetEstablishmentByIdAsync(id);

        if (establishmentEntity is null)
        {
            throw new KeyNotFoundException($"Establishment with id {id} not found");
        }

        return establishmentEntity.AsDto();
    }

    public Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel)
    {
        return _establishmentLinkRepository.RecordGroupSelection(userGroupSelectionModel);
    }
}

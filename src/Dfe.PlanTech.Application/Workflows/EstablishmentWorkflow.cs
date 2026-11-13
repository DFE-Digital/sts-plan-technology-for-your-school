using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Workflows;

public class EstablishmentWorkflow(
    ILogger<EstablishmentWorkflow> logger,
    IEstablishmentRepository establishmentRepository,
    IEstablishmentLinkRepository establishmentLinkRepository,
    IGiasRepository giasRepository
) : IEstablishmentWorkflow
{
    private readonly ILogger<EstablishmentWorkflow> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IEstablishmentRepository _establishmentRepository =
        establishmentRepository ?? throw new ArgumentNullException(nameof(establishmentRepository));
    private readonly IEstablishmentLinkRepository _establishmentLinkRepository =
        establishmentLinkRepository
        ?? throw new ArgumentNullException(nameof(establishmentLinkRepository));
    private readonly IGiasRepository _giasRepository =
        giasRepository ?? throw new ArgumentNullException(nameof(giasRepository));

    public async Task<SqlEstablishmentDto> GetOrCreateSchoolAsync(
        string schoolUrn,
        string schoolName
    )
    {
        if (!int.TryParse(schoolUrn, out var urn))
        {
            throw new InvalidDataException("Cannot parse an integer URN from the provided string");
        }

        var giasSchool = await _giasRepository.GetSchoolByUrn(urn);

        EstablishmentModel? establishmentModel = null;
        if (giasSchool is null)
        {
            _logger.LogWarning(
                "Could not find establishment with URN {Urn} in GIAS data. Creating establishment from minimal establishment model.",
                schoolUrn
            );

            establishmentModel = new EstablishmentModel { Name = schoolName, Urn = schoolUrn };
        }
        else
        {
            establishmentModel = new EstablishmentModel
            {
                Name = giasSchool.EstablishmentName,
                Type = giasSchool.TypeOfEstablishment?.TypeOfEstablishmentName is null
                    ? null
                    : new IdWithNameModel
                    {
                        Name = giasSchool.TypeOfEstablishment.TypeOfEstablishmentName,
                    },
                Ukprn = giasSchool.Ukprn,
                Urn = giasSchool.Urn.ToString(),
                GroupUid = null,
            };
        }

        return await GetOrCreateEstablishmentAsync(establishmentModel);
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

    public async Task<SqlEstablishmentDto?> GetEstablishmentByIdAsync(int id)
    {
        var establishment = await _establishmentRepository.GetEstablishmentByIdAsync(id);
        return establishment?.AsDto();
    }

    public async Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByIdsAsync(
        IEnumerable<int> establishmentIds
    )
    {
        var establishments = await _establishmentRepository.GetEstablishmentsByIdsAsync(
            establishmentIds
        );
        return establishments.Select(e => e.AsDto());
    }

    public async Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(
        string establishmentReference
    )
    {
        var establishment = await _establishmentRepository.GetEstablishmentByReferenceAsync(
            establishmentReference
        );
        return establishment?.AsDto();
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

using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Services;

public class EstablishmentService(
    IEstablishmentWorkflow establishmentWorkflow,
    IEstablishmentRepository establishmentRepository,
    IRecommendationWorkflow recommendationWorkflow,
    IUserWorkflow userWorkflow
) : IEstablishmentService
{
    private readonly IEstablishmentWorkflow _establishmentWorkflow =
        establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly IRecommendationWorkflow _recommendationWorkflow =
        recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));
    private readonly IUserWorkflow _userWorkflow =
        userWorkflow ?? throw new ArgumentNullException(nameof(userWorkflow));
    private readonly IEstablishmentRepository _establishmentRepository =
        establishmentRepository ?? throw new ArgumentNullException(nameof(establishmentRepository));

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

    public Task<List<SqlEstablishmentLinkDto>> GetEstablishmentLinks(int establishmentId)
    {
        return _establishmentWorkflow.GetGroupEstablishments(establishmentId);
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
        var establishmentEntity = await _establishmentRepository.GetEstablishmentByIdAsync(id);

        if (establishmentEntity is null)
        {
            throw new KeyNotFoundException($"Establishment with id {id} not found");
        }

        return establishmentEntity.AsDto();
    }

    public async Task<List<SqlEstablishmentLinkDto>> GetEstablishmentLinksWithRecommendationCounts(
        int establishmentId
    )
    {
        var establishmentLinks = await _establishmentWorkflow.GetGroupEstablishments(
            establishmentId
        );

        var linkUrns = establishmentLinks.Select(s => s.Urn);
        var establishments = await _establishmentWorkflow.GetEstablishmentsByReferencesAsync(
            linkUrns
        );

        var establishmentLinkMap = establishments
            .Where(e => e.EstablishmentRef is not null)
            .ToDictionary(e => e.EstablishmentRef!, e => e.Id);

        foreach (var establishmentLink in establishmentLinks)
        {
            if (!establishmentLinkMap.ContainsKey(establishmentLink.Urn))
            {
                establishmentLink.InProgressOrCompletedRecommendationsCount = 0;
                continue;
            }

            var schoolEstablishmentId = establishmentLinkMap[establishmentLink.Urn];
            var recommendations =
                await _recommendationWorkflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(
                    schoolEstablishmentId
                );

            establishmentLink.InProgressOrCompletedRecommendationsCount =
                recommendations.Values.Count(r =>
                    r.NewStatus == RecommendationStatus.Complete
                    || r.NewStatus == RecommendationStatus.InProgress
                );
        }

        return establishmentLinks.ToList();
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

        await _establishmentWorkflow.RecordGroupSelection(selectionModel);
    }
}

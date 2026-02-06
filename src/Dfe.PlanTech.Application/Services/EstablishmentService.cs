using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services;

public class EstablishmentService(
    IEstablishmentWorkflow establishmentWorkflow,
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

    public Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(
        EstablishmentModel establishmentModel
    )
    {
        return _establishmentWorkflow.GetOrCreateEstablishmentAsync(establishmentModel);
    }

    public async Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(
        string establishmentReference
    )
    {
        return await _establishmentWorkflow.GetEstablishmentByReferenceAsync(
            establishmentReference
        );
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

        var selectedEstablishment = await _establishmentWorkflow.GetEstablishmentByReferenceAsync(
            selectedEstablishmentUrn
        );
        selectedEstablishment ??= await _establishmentWorkflow.GetOrCreateEstablishmentAsync(
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

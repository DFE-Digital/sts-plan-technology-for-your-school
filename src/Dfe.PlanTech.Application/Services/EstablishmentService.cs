using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
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
    private readonly IEstablishmentWorkflow _establishmentWorkflow = establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly IRecommendationWorkflow _recommendationWorkflow = recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));
    private readonly IUserWorkflow _userWorkflow = userWorkflow ?? throw new ArgumentNullException(nameof(userWorkflow));

    public Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(EstablishmentModel establishmentModel)
    {
        return _establishmentWorkflow.GetOrCreateEstablishmentAsync(establishmentModel);
    }

    public async Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(string establishmentReference)
    {
        return await _establishmentWorkflow.GetEstablishmentByReferenceAsync(establishmentReference);
    }

    public async Task<List<SqlEstablishmentLinkDto>> GetEstablishmentLinksWithRecommendationCounts(IEnumerable<QuestionnaireSectionEntry> sections, int establishmentId)
    {
        var schools = await _establishmentWorkflow.GetGroupEstablishments(establishmentId);
        var schoolUrns = schools.Select(s => s.Urn);
        var establishments = await _establishmentWorkflow.GetEstablishmentsByReferencesAsync(schoolUrns);
        var establishmentLinkMap = establishments.ToDictionary(e => e.Id, e => schools.Single(s => s.Urn.Equals(e.EstablishmentRef)));

        foreach (var school in establishments)
        {
            var recommendations = await _recommendationWorkflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(school.Id);

            establishmentLinkMap[school.Id].InProgressOrCompletedRecommendationsCount = recommendations.Values.Count(r => r.NewStatus == nameof(RecommendationStatus.Complete).ToString() || r.NewStatus == nameof(RecommendationStatus.InProgress).ToString());
        }
        return establishmentLinkMap.Values.ToList();
    }

    public async Task RecordGroupSelection(
        string userDsiReference,
        int? userEstablishmentId,
        EstablishmentModel userEstablishmentModel,
        string selectedEstablishmentUrn,
        string selectedEstablishmentName)
    {
        var user = await _userWorkflow.GetUserBySignInRefAsync(userDsiReference)
            ?? throw new InvalidDataException("User does not exist");

        if (userEstablishmentId is null)
        {
            var userEstablishment = await _establishmentWorkflow.GetOrCreateEstablishmentAsync(userEstablishmentModel);
            userEstablishmentId = userEstablishment.Id;
        }

        var selectedEstablishment = await _establishmentWorkflow.GetEstablishmentByReferenceAsync(selectedEstablishmentUrn);
        selectedEstablishment ??= await _establishmentWorkflow.GetOrCreateEstablishmentAsync(selectedEstablishmentUrn, selectedEstablishmentName);

        var selectionModel = new UserGroupSelectionModel
        {
            SelectedEstablishmentId = selectedEstablishment.Id,
            SelectedEstablishmentName = selectedEstablishmentName,
            UserEstablishmentId = userEstablishmentId.Value,
            UserId = user.Id
        };

        await _establishmentWorkflow.RecordGroupSelection(selectionModel);
    }
}

using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Application.Services;

public class GroupService(IGroupWorkflow groupWorkflow) : IGroupService
{
    private readonly IGroupWorkflow _groupWorkflow =
        groupWorkflow ?? throw new ArgumentNullException(nameof(groupWorkflow));

    public async Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissionsBySections(int[] establishmentIds)
    {
        var submissions = await _groupWorkflow.GetGroupCompletedSubmissions(establishmentIds);
        return submissions;
    }

    public async Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(List<SqlEstablishmentLinkDto> establishmentLinks, string sectionId)
    {
        var submissions = await _groupWorkflow.GetGroupSubmissionInformationForSection(establishmentLinks, sectionId);
        return submissions;
    }

    public async
        Task<List<(EstablishmentEntity establishment, EstablishmentRecommendationHistoryEntity? recommendationHistory)>>
        GetLatestGroupEstablishmentRecommendationHistoryByRecommendationId(int establishmentId, int recommendationId)
    {
        var results = await _groupWorkflow.GetLatestGroupEstablishmentRecommendationHistoryByRecommendationId(establishmentId, recommendationId);
        return results;
    }

    public Task<GroupEstablishmentModel> GetGroupEstablishmentContextAsync(
    int groupEstablishmentId
    )
    {
        return _groupWorkflow.GetGroupEstablishmentContextAsync(groupEstablishmentId);
    }
}

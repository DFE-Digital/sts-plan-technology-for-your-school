using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Services;

public class GroupService(IGroupWorkflow groupWorkflow, IGiasRepository giasRepository, IRecommendationWorkflow recommendationWorkflow) : IGroupService
{
    private readonly IGroupWorkflow _groupWorkflow =
        groupWorkflow ?? throw new ArgumentNullException(nameof(groupWorkflow));
    private readonly IRecommendationWorkflow _recommendationWorkflow =
    recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));
    private readonly IGiasRepository _giasRepository =
    giasRepository ?? throw new ArgumentNullException(nameof(giasRepository));

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

    public async Task<GroupEstablishmentDTO?> GetGroupWithEstablishmentsBasic(int groupEstId)
    {
        var dboGroup = await _groupWorkflow.GetGroupFromDboEstablishmentAsync(groupEstId);
        var groupUid = 0;
        int.TryParse(dboGroup?.GroupUid, out groupUid);
        var groupDTO = await _giasRepository.GetGiasGroupByGroupUIDAsync(groupUid);
        var recHistories = await _recommendationWorkflow.GetRecommendationInProgressOrCompletedRecommendationsCount(groupDTO?.BasicEstablishments.Select(e => e.Urn) ?? []);
        groupDTO?.BasicEstablishments.ForEach(e => e.InProgressOrCompletedRecommendationsCount = recHistories[e.Urn]);
        return groupDTO;
    }
}

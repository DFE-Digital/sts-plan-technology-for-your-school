using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Services;

public class GroupService(IGroupWorkflow groupWorkflow, IGiasRepository giasRepository,
    IRecommendationWorkflow recommendationWorkflow, IEstablishmentService establishmentService) : IGroupService
{
    private readonly IGroupWorkflow _groupWorkflow =
        groupWorkflow ?? throw new ArgumentNullException(nameof(groupWorkflow));
    private readonly IRecommendationWorkflow _recommendationWorkflow =
    recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));
    private readonly IEstablishmentService _establishmentService =
    establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly IGiasRepository _giasRepository =
    giasRepository ?? throw new ArgumentNullException(nameof(giasRepository));

    /// <summary>
    /// Get the total count of submissions per section id across the whole group
    /// </summary>
    /// <param name="dboGroupId"></param>
    /// <param name="statuses"></param>
    /// <returns></returns>
    public async Task<Dictionary<string,int>> GetGroupCompletedSubmissionCountBySection(int dboGroupId)
    {
        //1. get linked urns vias gias
        var groupUid = await GetGroupUIDAsync(dboGroupId);
        var urns = await _giasRepository.GetLinkedURNSForGroupAsync(dboGroupId) ?? [];
        var urnStrs = urns.Select(u => u.ToString());
        //2.get the submissions counts
        return await _groupWorkflow.GetGroupSubmissionsCountBySectionsFromUrns(urnStrs, SubmissionStatus.CompleteReviewed);
    }

    public async Task<Dictionary<string, int>> GetGroupCompletedSubmissionCountBySection(IEnumerable<int> dboSchoolIds)
    {
        return await _groupWorkflow.GetGroupSubmissionsCountBySectionsFromIds(dboSchoolIds, SubmissionStatus.CompleteReviewed);
    }

    public async Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(int dboGroupId, string sectionId)
    {
        var group = await GetGroupWithEstablishmentsFromGIASAndCreateInDbo(dboGroupId);
        return group?.BasicEstablishments.Count != 0 ? await _groupWorkflow.GetGroupSubmissionInformationForSection(group!.BasicEstablishments, sectionId) : new List<SubmissionInformationModel>();
    }

    private async Task<int> GetGroupUIDAsync(int dboGroupId)
    {
        var dboGroup = await _groupWorkflow.GetGroupFromDboEstablishmentAsync(dboGroupId);
        var groupUid = 0;
        int.TryParse(dboGroup?.GroupUid, out groupUid);
        return groupUid;
    }

    public async Task<GroupEstablishmentDTO?> GetGroupWithEstablishmentsFromGIASAndCreateInDbo(int dboGroupId)
    {
        var groupUid = await GetGroupUIDAsync(dboGroupId);
        var groupDTO = await _giasRepository.GetGiasGroupByGroupUIDAsync(groupUid);
        if (groupDTO != null)
        {
            foreach (var e in groupDTO.BasicEstablishments)
            {
                if (e.DboId.HasValue)
                {
                    var est =
                        await _establishmentService.GetOrCreateEstablishmentAsync(
                            e.Urn,
                            e.Name);
                    e.DboId = est.Id;
                }

            }
        }
        return groupDTO;
    }

    public async Task<GroupEstablishmentDTO?> GetGroupHomePageModel(int dboGroupId)
    {
        var groupDTO = await GetGroupWithEstablishmentsFromGIASAndCreateInDbo(dboGroupId);
        var recHistories = await _recommendationWorkflow.GetRecommendationInProgressOrCompletedRecommendationsCount(groupDTO?.BasicEstablishments.Select(e => e.Urn) ?? []);
        groupDTO?.BasicEstablishments.ForEach(e => e.InProgressOrCompletedRecommendationsCount = recHistories[e.Urn]);
        return groupDTO;
    }

    /// <summary>
    /// URN must be present in GIAS group membership
    /// </summary>
    /// <param name="dboGroupId"></param>
    /// <param name="urn"></param>
    /// <returns></returns>
    public async Task<bool> IsSchoolWithinGroup(int dboGroupId, string urn)
    {
        var groupUid = await GetGroupUIDAsync(dboGroupId);
        return await _giasRepository.IsSchoolWithinGroup(groupUid, urn);
    }

    /// <summary>
    /// any one URN must be present in GIAS group membership
    /// </summary>
    /// <param name="dboGroupId"></param>
    /// <param name="urn"></param>
    /// <returns></returns>
    public async Task<bool> IsSchoolWithinGroup(int dboGroupId, IEnumerable<string> urns)
    {
        var groupUid = await GetGroupUIDAsync(dboGroupId);
        return await _giasRepository.IsSchoolWithinGroup(groupUid, urns);
    }

    /// <summary>
    /// All URNS must be present in GIAS group membership
    /// </summary>
    /// <param name="dboGroupId"></param>
    /// <param name="urns"></param>
    /// <returns></returns>
    public async Task<bool> AreAllSchoolsWithinGroup(int dboGroupId, IEnumerable<string> urns)
    {
        var groupUid = await GetGroupUIDAsync(dboGroupId);
        return await _giasRepository.AreAllSchoolsWithinGroup(groupUid, urns);
    }
}

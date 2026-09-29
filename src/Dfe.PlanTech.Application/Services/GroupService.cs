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

    public async Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissionsBySections(IEnumerable<int> establishmentIds)
    {
        var submissions = await _groupWorkflow.GetGroupSubmissionsBySections(establishmentIds, SubmissionStatus.CompleteReviewed);
        return submissions;
    }

    /// <summary>
    /// Get the total count of submissions per section id across the whole group using group dboID
    /// </summary>
    /// <param name="dboGroupId"></param>
    /// <param name="statuses"></param>
    /// <returns></returns>
    public async Task<Dictionary<string,int>> GetGroupCompletedSubmissionCountBySection(int dboGroupId)
    {
        return await _groupWorkflow.GetGroupSubmissionsCountBySections(dboGroupId, SubmissionStatus.CompleteReviewed);
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

    public async Task<GroupEstablishmentDTO?> GetGroupWithEstablishmentsFromGIASAndCreateInDbo(int dboGroupId)
    {
        return await _groupWorkflow.GetGroupWithEstablishmentsFromGIASAndCreateInDbo(dboGroupId);
    }

    public async Task<GroupEstablishmentDTO?> GetGroupHomePageModel(int dboGroupId)
    {
        return await _groupWorkflow.GetGroupHomePageModel(dboGroupId);
    }

    /// <summary>
    /// URN must be present in GIAS group membership
    /// </summary>
    /// <param name="dboGroupId"></param>
    /// <param name="urn"></param>
    /// <returns></returns>
    public async Task<bool> IsSchoolWithinGroup(int dboGroupId, string urn)
    {
        return await _groupWorkflow.IsSchoolWithinGroup(dboGroupId, urn);
    }

    /// <summary>
    /// any one URN must be present in GIAS group membership
    /// </summary>
    /// <param name="dboGroupId"></param>
    /// <param name="urn"></param>
    /// <returns></returns>
    public async Task<bool> IsSchoolWithinGroup(int dboGroupId, IEnumerable<string> urns)
    {
        return await _groupWorkflow.IsSchoolWithinGroup(dboGroupId, urns);
    }

    /// <summary>
    /// All URNS must be present in GIAS group membership
    /// </summary>
    /// <param name="dboGroupId"></param>
    /// <param name="urns"></param>
    /// <returns></returns>
    public async Task<bool> AreAllSchoolsWithinGroup(int dboGroupId, IEnumerable<string> urns)
    {
        return await _groupWorkflow.AreAllSchoolsWithinGroup(dboGroupId, urns);
    }
}

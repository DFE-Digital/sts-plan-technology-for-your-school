using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class GroupWorkflow(ISubmissionRepository submissionRepository, IGiasRepository giasRepository,
    IEstablishmentWorkflow establishmentWorkflow, IRecommendationWorkflow recommendationWorkflow) : IGroupWorkflow
{
    private readonly ISubmissionRepository _submissionRepository =
        submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
    private readonly IGiasRepository _giasRepository =
        giasRepository ?? throw new ArgumentNullException(nameof(giasRepository));
    private readonly IEstablishmentWorkflow _establishmentWorkflow =
        establishmentWorkflow ?? throw new ArgumentNullException(nameof(establishmentWorkflow));
    private readonly IRecommendationWorkflow _recommendationWorkflow =
        recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));

    public async Task<List<SqlSubmissionDto>> GetGroupSubmissionsBySections(IEnumerable<int> establishmentIds, SubmissionStatus status = SubmissionStatus.CompleteReviewed, string? sectionId = null)
    {
        var submissions = await _submissionRepository.GetLatestEstablishmentsSubmissionsByEstablishmentAndSectionAsync(establishmentIds, status, sectionId);

        return submissions.Select(s => s.AsDto()).ToList();
    }

    public async Task<Dictionary<string, int>> GetGroupSubmissionsCountBySections(int dboGroupId, SubmissionStatus status = SubmissionStatus.CompleteReviewed, string? sectionId = null)
    {
        //1. get linked urns vias gias
        var groupUid = await GetGroupUIDAsync(dboGroupId);
        var urns = await _giasRepository.GetLinkedURNSForGroupAsync(groupUid) ?? [];
        var urnStrs = urns.Select(u => u.ToString());
        return await _submissionRepository.GetSubmissionsCountBySectionAsync(urns, status, sectionId    );
    }
    public async Task<Dictionary<string, int>> GetGroupSubmissionsCountBySectionsFromIds(IEnumerable<int> dboSchoolIds, SubmissionStatus status = SubmissionStatus.CompleteReviewed, string? sectionId = null)
    {
        return await _submissionRepository.GetSubmissionsCountBySectionAsync(dboSchoolIds, status, sectionId);
    }

    public async Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(IEnumerable<EstablishmentBasicDto> establishments, string sectionId)
    {
        var dboSchoolIds = establishments.Select(e => e?.DboId ?? 0).ToList() ?? [];
        var groupLatestSubmissions = await _submissionRepository.GetLatestSubmissionPerEstablishmentForSectionAsync(dboSchoolIds, sectionId)
            ?? [];

        var groupSubmissionInfo = new List<SubmissionInformationModel>();

        foreach (var est in establishments)
        {
            var submission = groupLatestSubmissions
                    .FirstOrDefault(s => s.Establishment.EstablishmentRef == est.Urn);

            if (submission == null)
            {
                groupSubmissionInfo.Add(
                    new SubmissionInformationModel
                    {
                        EstablishmentId = est.DboId.HasValue ? est.DboId.Value : 0,
                        EstablishmentName = est.Name ?? string.Empty,
                        EstablishmentRef = est.Urn ?? string.Empty,
                        SectionId = sectionId,
                        Status = SubmissionStatus.NotStarted
                    }
                );
            }
            else
            {
                groupSubmissionInfo.Add(
                    new SubmissionInformationModel
                    {
                        EstablishmentId = submission.EstablishmentId,
                        EstablishmentName = est.Name ?? "",
                        EstablishmentRef = est.Urn ?? "",
                        SectionId = sectionId,
                        SubmissionId = submission.Id,
                        DateCreated = DateTimeHelper.FormattedDateShort(submission.DateCreated),
                        DateLastUpdated = submission.DateLastUpdated != null
                            ? DateTimeHelper.FormattedDateShort(submission.DateLastUpdated.Value)
                            : null,
                        DateCompleted = submission.DateCompleted != null
                            ? DateTimeHelper.FormattedDateShort(submission.DateCompleted.Value)
                            : null,
                        Status = submission.Status
                    }
                );
            }
        }

        return groupSubmissionInfo;
    }



    public async Task<GroupEstablishmentDTO?> GetGroupWithEstablishmentsFromGIASAndCreateInDbo(int dboGroupId)
    {
        var groupUid = await GetGroupUIDAsync(dboGroupId);
        var groupDTO = await _giasRepository.GetGiasGroupByGroupUIDAsync(groupUid);
        if (groupDTO != null)
        {
            foreach (var e in groupDTO.BasicEstablishments)
            {
                if (!e.DboId.HasValue)
                {
                    var est =
                        await _establishmentWorkflow.GetOrCreateEstablishmentAsync(
                            e.Urn,
                            e.Name);
                    e.DboId = est.Id;
                }

            }
        }
        return groupDTO;
    }
    private async Task<int> GetGroupUIDAsync(int dboGroupId)
    {
        var dboGroup = await GetGroupFromDboEstablishmentAsync(dboGroupId);
        var groupUid = 0;
        int.TryParse(dboGroup?.GroupUid, out groupUid);
        return groupUid;
    }

    public async Task<SqlEstablishmentDto?> GetGroupFromDboEstablishmentAsync(int id)
    {
        return await _establishmentWorkflow.GetEstablishmentByIdAsync(id);
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

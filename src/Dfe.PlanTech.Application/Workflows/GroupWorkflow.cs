using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class GroupWorkflow(ISubmissionRepository submissionRepository, IEstablishmentService establishmentService, IEstablishmentRecommendationHistoryRepository recommendationHistoryRepository, IGiasRepository giasRepository, IRecommendationWorkflow recommendationWorkflow) : IGroupWorkflow
{
    private readonly IGiasRepository _giasRepository = giasRepository ?? throw new ArgumentNullException(nameof(giasRepository));
    private readonly IRecommendationWorkflow _recommendationWorkflow = recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));
    private readonly ISubmissionRepository _submissionRepository =
        submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
    private readonly IEstablishmentService _establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));

    private readonly IEstablishmentRecommendationHistoryRepository _recommendationHistoryRepository = recommendationHistoryRepository ?? throw new ArgumentNullException(nameof(recommendationHistoryRepository));

    public async Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissions(int[] establishmentIds)
    {
        var submissions = await _submissionRepository.GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(establishmentIds);

        return submissions.Select(s => s.AsDto()).ToList();
    }

    public async Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(List<SqlEstablishmentLinkDto> establishmentLinks, string sectionId)
    {
        var establishments = new List<SqlEstablishmentDto>();

        foreach (var e in establishmentLinks)
        {
            establishments.Add(
                await _establishmentService.GetOrCreateEstablishmentAsync(
                    e.Urn,
                    e.EstablishmentName));
        }

        var establishmentIds = establishments
            .Select(e => e.Id)
            .Distinct()
            .ToArray();

        var groupLatestSubmissions = await _submissionRepository.GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId)
            ?? [];

        var groupSubmissionInfo = new List<SubmissionInformationModel>();

        foreach (var est in establishments)
        {
            var submission = groupLatestSubmissions
                    .FirstOrDefault(s => s.Establishment.EstablishmentRef == est.EstablishmentRef);

            if (submission == null)
            {
                groupSubmissionInfo.Add(
                    new SubmissionInformationModel
                    {
                        EstablishmentId = est.Id,
                        EstablishmentName = est.OrgName ?? "",
                        EstablishmentRef = est.EstablishmentRef ?? "",
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
                        EstablishmentName = est.OrgName ?? "",
                        EstablishmentRef = est.EstablishmentRef ?? "",
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

    public async Task<List<(EstablishmentEntity establishment, EstablishmentRecommendationHistoryEntity? recommendationHistory)>> GetLatestGroupEstablishmentRecommendationHistoryByRecommendationId(int establishmentId, int recommendationId)
    {
        var results =
            await _recommendationHistoryRepository.GetLatestGroupRecommendationHistoryByRecommendationIdAsync(establishmentId,
                recommendationId);

        return results;

    }

        private async Task<int> GetGroupUidOrThrowAsync(int groupEstablishmentId)
    {
        var group = await _establishmentService.GetEstablishmentByIdAsync(groupEstablishmentId);
        if (!int.TryParse(group.GroupUid, out var groupUid) || groupUid <= 0)
            throw new InvalidDataException($"Group {groupEstablishmentId} has no valid GroupUid");
        return groupUid;
    }

    public async Task<bool> IsSchoolWithinGroupAsync(int groupEstablishmentId, string schoolUrn)
    {
        if (!int.TryParse(schoolUrn, out var urn) || urn <= 0)
            return false;
        var groupUid = await GetGroupUidOrThrowAsync(groupEstablishmentId);
        return await _giasRepository.IsSchoolWithinGroupAsync(groupUid, urn);
    }

    public async Task<List<SqlEstablishmentLinkDto>> GetGroupSchoolsAsync(int groupEstablishmentId, bool includeRecommendationCounts = false)
    {
        var establishments = await ResolveGroupEstablishmentsAsync(groupEstablishmentId);
        var schools = new List<SqlEstablishmentLinkDto>();
        foreach (var school in establishments)
        {
            var dto = new SqlEstablishmentLinkDto
            {
                Id = school.Id,
                Urn = school.EstablishmentRef,
                EstablishmentName = school.OrgName ?? string.Empty,
                GroupUid = school.GroupUid ?? string.Empty
            };
            if (includeRecommendationCounts)
            {
                var statuses = await _recommendationWorkflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(school.Id);
                dto.InProgressOrCompletedRecommendationsCount = statuses.Values.Count(r =>
                    r.NewStatus == RecommendationStatus.Complete ||
                    r.NewStatus == RecommendationStatus.InProgress);
            }
            schools.Add(dto);
        }
        return schools;
    }

    public async Task<GroupEstablishmentModel> GetGroupEstablishmentContextAsync(int groupEstablishmentId)
    {
        var establishments = await ResolveGroupEstablishmentsAsync(groupEstablishmentId);
        var ids = establishments.Select(e => e.Id).Distinct().ToArray();
        var completed = ids.Length == 0 ? [] : await GetGroupCompletedSubmissions(ids);
        return new GroupEstablishmentModel
        {
            Establishments = establishments,
            EstablishmentIds = ids,
            CompletedSubmissions = completed
        };
    }

    private async Task<List<SqlEstablishmentDto>> ResolveGroupEstablishmentsAsync(int groupEstablishmentId)
    {
        var groupUid = await GetGroupUidOrThrowAsync(groupEstablishmentId);
        var giasSchools = (await _giasRepository.GetSchoolsForGroupAsync(groupUid))
            .DistinctBy(school => school.Urn)
            .ToList();

        if (giasSchools.Count == 0)
            return [];

        var existing = await _establishmentService.GetEstablishmentsByReferencesAsync(
            giasSchools.Select(school => school.Urn.ToString()));
        var byUrn = existing
            .Where(establishment => establishment.EstablishmentRef != null)
            .GroupBy(establishment => establishment.EstablishmentRef!)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        var establishments = new List<SqlEstablishmentDto>(giasSchools.Count);
        foreach (var school in giasSchools)
        {
            var urn = school.Urn.ToString();
            if (!byUrn.TryGetValue(urn, out var establishment))
            {
                establishment = await _establishmentService.GetOrCreateEstablishmentAsync(
                    urn, school.EstablishmentName);
                byUrn[urn] = establishment;
            }

            establishments.Add(establishment);
        }
        return establishments;
    }
}

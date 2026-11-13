using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class GroupWorkflow(
    IEstablishmentGroupRepository establishmentGroupRepository,
    IEstablishmentRecommendationHistoryRepository recommendationHistoryRepository,
    ISubmissionRepository submissionRepository
) : IGroupWorkflow
{
    private readonly IEstablishmentGroupRepository _establishmentGroupRepository =
        establishmentGroupRepository
        ?? throw new ArgumentNullException(nameof(establishmentGroupRepository));
    private readonly IEstablishmentRecommendationHistoryRepository _recommendationHistoryRepository =
        recommendationHistoryRepository
        ?? throw new ArgumentNullException(nameof(recommendationHistoryRepository));
    private readonly ISubmissionRepository _submissionRepository =
        submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));

    public async Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissions(int[] establishmentIds)
    {
        var submissions =
            await _submissionRepository.GetEstablishmentsLatestCompletedSubmissionsBySectionsAsync(
                establishmentIds
            );
        return submissions.Select(s => s.AsDto()).ToList();
    }

    public async Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(
        List<SelectedEstablishmentDataModel> groupSchools,
        string sectionId
    )
    {
        var establishmentIds = groupSchools.Select(e => e.EstablishmentId).Distinct().ToArray();

        var groupLatestSubmissions =
            await _submissionRepository.GetLatestSubmissionPerEstablishmentForSectionAsync(
                establishmentIds,
                sectionId
            ) ?? [];

        var groupSubmissionInfo = new List<SubmissionInformationModel>();

        foreach (var school in groupSchools)
        {
            var submission = groupLatestSubmissions.FirstOrDefault(s =>
                s.EstablishmentId == school.EstablishmentId
            );
            if (submission == null)
            {
                groupSubmissionInfo.Add(
                    new SubmissionInformationModel
                    {
                        EstablishmentId = school.EstablishmentId,
                        EstablishmentName = school.Name,
                        EstablishmentRef = school.Urn,
                        SectionId = sectionId,
                        Status = SubmissionStatus.NotStarted,
                    }
                );
            }
            else
            {
                groupSubmissionInfo.Add(
                    new SubmissionInformationModel
                    {
                        EstablishmentId = school.EstablishmentId,
                        EstablishmentName = school.Name,
                        EstablishmentRef = school.Urn,
                        SectionId = sectionId,
                        SubmissionId = submission.Id,
                        DateCreated = DateTimeHelper.FormattedDateShort(submission.DateCreated),
                        DateLastUpdated =
                            submission.DateLastUpdated != null
                                ? DateTimeHelper.FormattedDateShort(
                                    submission.DateLastUpdated.Value
                                )
                                : null,
                        DateCompleted =
                            submission.DateCompleted != null
                                ? DateTimeHelper.FormattedDateShort(submission.DateCompleted.Value)
                                : null,
                        Status = submission.Status,
                    }
                );
            }
        }

        return groupSubmissionInfo;
    }

    public async Task<
        List<(
            EstablishmentEntity establishment,
            EstablishmentRecommendationHistoryEntity? recommendationHistory
        )>
    > GetLatestGroupEstablishmentRecommendationHistoryByRecommendationId(
        int establishmentId,
        int recommendationId
    )
    {
        var results =
            await _recommendationHistoryRepository.GetLatestGroupRecommendationHistoryByRecommendationIdAsync(
                establishmentId,
                recommendationId
            );

        return results;
    }

    //public async Task<GroupEstablishmentModel> GetGroupEstablishmentContextAsync(
    //    int groupEstablishmentId
    //)
    //{
    //    var establishments =
    //        await _establishmentGroupRepository
    //            .GetLinkedEstablishmentsByGroupEstablishmentIdAsync(groupEstablishmentId);

    //    var establishmentIds = establishments
    //        .Select(e => e.Id)
    //        .Distinct()
    //        .ToArray();

    //    var completedSubmissions =
    //        establishmentIds.Length != 0
    //            ? await GetGroupCompletedSubmissions(establishmentIds)
    //            : [];

    //    return new GroupEstablishmentModel
    //    {
    //        Establishments = establishments
    //            .Select(e => e.AsDto())
    //            .ToList(),
    //        EstablishmentIds = establishmentIds,
    //        CompletedSubmissions = completedSubmissions,
    //    };
    //}
}

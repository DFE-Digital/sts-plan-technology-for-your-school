using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;

namespace Dfe.PlanTech.Application.Services;

public class SubmissionService(
    IRecommendationWorkflow recommendationWorkflow,
    ISubmissionWorkflow submissionWorkflow
) : ISubmissionService
{
    private readonly IRecommendationWorkflow _recommendationWorkflow = recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));
    private readonly ISubmissionWorkflow _submissionWorkflow = submissionWorkflow ?? throw new ArgumentNullException(nameof(submissionWorkflow));

    public async Task<SqlSubmissionDto> RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(int establishmentId, QuestionnaireSectionEntry section)
    {
        // Check if an in-progress submission already exists
        var inProgressSubmission = await _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(
            establishmentId,
            section,
            isCompletedSubmission: false);

        if (inProgressSubmission is not null)
        {
            await _submissionWorkflow.SetSubmissionInaccessibleAsync(inProgressSubmission.Id);
        }

        return await _submissionWorkflow.CloneLatestCompletedSubmission(establishmentId, section);
    }

    public Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesByRecommendationIdAsync(
        IEnumerable<string> recommendationContentfulReferences,
        int establishmentId
    )
    {
        return _recommendationWorkflow.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationContentfulReferences, establishmentId);
    }

    public async Task<SubmissionResponsesModel?> GetLatestSubmissionResponsesModel(int establishmentId, QuestionnaireSectionEntry section, bool isCompletedSubmission)
    {
        var submission = await _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, isCompletedSubmission);
        return submission is null
            ? null
            : new SubmissionResponsesModel(submission, section);
    }

    public Task<SqlSubmissionDto> GetSubmissionByIdAsync(int submissionId)
    {
        return _submissionWorkflow.GetSubmissionByIdAsync(submissionId);
    }

    public async Task<SubmissionRoutingDataModel> GetSubmissionRoutingDataAsync(int establishmentId, QuestionnaireSectionEntry section, bool? isCompletedSubmission)
    {
        var latestSubmission = await _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(
            establishmentId,
            section,
            isCompletedSubmission);

        SubmissionStatus status;

        if (latestSubmission == null || (latestSubmission.Status != null && latestSubmission.Status.Equals(SubmissionStatus.Inaccessible)))
        {
            status = SubmissionStatus.NotStarted;
        }
        else if (latestSubmission.Completed)
        {
            status = SubmissionStatus.CompleteReviewed;
        }
        else
        {
            status = SubmissionStatus.InProgress;
        }

        var submissionResponsesModel = latestSubmission is null || (latestSubmission.Status != null && latestSubmission.Status.Equals(SubmissionStatus.Inaccessible))
            ? null
            : new SubmissionResponsesModel(latestSubmission, section);

        if (status.Equals(SubmissionStatus.NotStarted))
        {
            return new SubmissionRoutingDataModel
            (
                nextQuestion: section.Questions.First(),
                questionnaireSection: section,
                submission: submissionResponsesModel,
                status
            );
        }

        var lastResponse = submissionResponsesModel!.Responses.Last();
        var cmsLastAnswer = section.Questions
            .FirstOrDefault(q => q.Id.Equals(lastResponse.QuestionSysId))?
            .Answers
            .FirstOrDefault(a => a.Id.Equals(lastResponse.AnswerSysId));

        var sectionStatus = latestSubmission?.Status is null
            ? cmsLastAnswer?.NextQuestion is null
               ? SubmissionStatus.CompleteNotReviewed
               : SubmissionStatus.InProgress
            : latestSubmission.Status.ToSubmissionStatus();

        return new SubmissionRoutingDataModel
        (
            nextQuestion: cmsLastAnswer?.NextQuestion,
            questionnaireSection: section,
            submission: submissionResponsesModel,
            status: sectionStatus
        );
    }

    public Task<List<SqlSectionStatusDto>> GetSectionStatusesForSchoolAsync(int establishmentId, IEnumerable<string> sectionIds)
    {
        return _submissionWorkflow.GetSectionStatusesAsync(establishmentId, sectionIds);
    }

    public Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId)
    {
        return _submissionWorkflow.SetLatestSubmissionViewedAsync(establishmentId, sectionId);
    }

    public Task<int> SubmitAnswerAsync(int userId, int establishmentId, int? matEstablishmentId, SubmitAnswerModel answerModel)
    {
        return _submissionWorkflow.SubmitAnswer(userId, establishmentId, matEstablishmentId, answerModel);
    }

    public Task ConfirmCheckAnswersAsync(int submissionId)
    {
        return _submissionWorkflow.SetMaturityAndMarkAsReviewedAsync(submissionId);
    }

    public Task ConfirmCheckAnswersAndUpdateRecommendationsAsync(int establishmentId, int? matEstablishmentId, int submissionId, int userId, QuestionnaireSectionEntry section)
    {
        return _submissionWorkflow.ConfirmCheckAnswersAndUpdateRecommendationsAsync(establishmentId, matEstablishmentId, submissionId, userId, section);
    }

    public async Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId)
    {
        await _submissionWorkflow.SetSubmissionInaccessibleAsync(establishmentId, sectionId);
    }

    public async Task RestoreInaccessibleSubmissionAsync(int establishmentId, string sectionId)
    {
        await _submissionWorkflow.SetSubmissionInProgressAsync(establishmentId, sectionId);
    }
}

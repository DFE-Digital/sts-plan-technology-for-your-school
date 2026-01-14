using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
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
            status: SubmissionStatus.InProgress);

        if (inProgressSubmission is not null)
        {
            await _submissionWorkflow.SetSubmissionInaccessibleAsync(inProgressSubmission.Id);
        }

        return await _submissionWorkflow.CloneLatestCompletedSubmission(establishmentId, section);
    }

    public Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesByEstablishmentIdAsync(
        int establishmentId
    )
    {
        return _recommendationWorkflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(establishmentId);
    }

    public async Task<SubmissionResponsesModel?> GetLatestSubmissionResponsesModel(int establishmentId, QuestionnaireSectionEntry section, SubmissionStatus status)
    {
        var submission = await _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, status);
        return submission is null
            ? null
            : new SubmissionResponsesModel(submission, section);
    }

    public Task<SqlSubmissionDto> GetSubmissionByIdAsync(int submissionId)
    {
        return _submissionWorkflow.GetSubmissionByIdAsync(submissionId);
    }

    public async Task<SubmissionRoutingDataModel> GetSubmissionRoutingDataAsync(
        int establishmentId,
        QuestionnaireSectionEntry section,
        SubmissionStatus? status)
    {
        var latestSubmission = await _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(
            establishmentId,
            section,
            status);

        bool isNullSubmissionOrInvalidStatus =
            latestSubmission == null ||
            latestSubmission.Status == SubmissionStatus.Inaccessible ||
            latestSubmission.Status == SubmissionStatus.Obsolete;

        if (isNullSubmissionOrInvalidStatus)
        {
            return new SubmissionRoutingDataModel
            (
                nextQuestion: section.Questions.First(),
                questionnaireSection: section,
                submission: null,
                status: SubmissionStatus.NotStarted
            );
        }

        var submissionResponsesModel = new SubmissionResponsesModel(latestSubmission!, section);

        var lastResponse = submissionResponsesModel.Responses.Last();
        var cmsLastAnswer = section.Questions
            .FirstOrDefault(q => q.Id.Equals(lastResponse.QuestionSysId))?
            .Answers
            .FirstOrDefault(a => a.Id.Equals(lastResponse.AnswerSysId));

        SubmissionStatus sectionStatus;

        if (latestSubmission!.Status == SubmissionStatus.None)
        {
            sectionStatus = cmsLastAnswer?.NextQuestion is null
                ? SubmissionStatus.CompleteNotReviewed
                : SubmissionStatus.InProgress;
        }
        else
        {
            sectionStatus = latestSubmission.Status;
        }

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

    public Task<int> SubmitAnswerAsync(int userId, int activeEstablishmentId, int userEstablishmentId, SubmitAnswerModel answerModel)
    {
        return _submissionWorkflow.SubmitAnswer(userId, activeEstablishmentId, userEstablishmentId, answerModel);
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

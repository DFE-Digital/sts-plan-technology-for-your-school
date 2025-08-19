using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;

namespace Dfe.PlanTech.Application.Services;

public class SubmissionService(
    ContentfulWorkflow contentfulWorkflow,
    SubmissionWorkflow submissionWorkflow
)
{
    private readonly ContentfulWorkflow _contentfulWorkflow = contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));
    private readonly SubmissionWorkflow _submissionWorkflow = submissionWorkflow ?? throw new ArgumentNullException(nameof(submissionWorkflow));

    public async Task RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(int establishmentId, QuestionnaireSectionEntry section)
    {
        // Check if an in-progress submission already exists
        var inProgressSubmission = await _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(
            establishmentId,
            section,
            isCompletedSubmission: false);
        if (inProgressSubmission != null)
        {
            await _submissionWorkflow.DeleteSubmissionSoftAsync(inProgressSubmission.Id);
        }

        await _submissionWorkflow.CloneLatestCompletedSubmission(establishmentId, section.Sys.Id);
    }

    public async Task<SubmissionResponsesModel?> GetLatestSubmissionResponsesModel(int establishmentId, QuestionnaireSectionEntry section, bool isCompletedSubmission)
    {
        var submission = await _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, isCompletedSubmission);
        return submission is null
            ? null
            : new SubmissionResponsesModel(submission, section);
    }

    public async Task<SubmissionRoutingDataModel> GetSubmissionRoutingDataAsync(int establishmentId, string sectionSlug, bool? isCompletedSubmission)
    {
        var section = await _contentfulWorkflow.GetSectionBySlugAsync(sectionSlug);

        var latestCompletedSubmission = await _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(
            establishmentId,
            section,
            isCompletedSubmission);

        var status = latestCompletedSubmission is null
            ? SubmissionStatus.NotStarted
            : latestCompletedSubmission.Completed
                ? SubmissionStatus.CompleteReviewed
                : SubmissionStatus.InProgress;

        var submissionResponsesModel = latestCompletedSubmission is null
            ? null
            : new SubmissionResponsesModel(latestCompletedSubmission, section);

        if (status.Equals(SubmissionStatus.NotStarted))
        {
            return new SubmissionRoutingDataModel
            (
                maturity: latestCompletedSubmission?.Maturity,
                nextQuestion: section.Questions.First(),
                questionnaireSection: section,
                submission: submissionResponsesModel,
                status
            );
        }

        var lastResponse = submissionResponsesModel!.Responses.Last();
        var cmsLastAnswer = section.Questions
            .FirstOrDefault(q => q.Sys.Id.Equals(lastResponse.QuestionSysId))?
            .Answers
            .FirstOrDefault(a => a.Sys.Id.Equals(lastResponse.AnswerSysId));

        if (!Enum.TryParse<SubmissionStatus>(latestCompletedSubmission?.Status, out var sectionStatus))
        {
            sectionStatus = cmsLastAnswer?.NextQuestion is null
               ? SubmissionStatus.CompleteNotReviewed
               : SubmissionStatus.InProgress;
        }

        return new SubmissionRoutingDataModel
        (
            maturity: submissionResponsesModel.Maturity,
            nextQuestion: cmsLastAnswer?.NextQuestion ?? section.Questions.First(),
            questionnaireSection: section,
            submission: submissionResponsesModel,
            status: sectionStatus
        );
    }

    public Task<List<SqlSectionStatusDto>> GetSectionStatusesForSchoolAsync(QuestionnaireCategoryEntry category, int establishmentId)
    {
        var sectionIds = category.Sections.Select(s => s.Sys.Id);
        return _submissionWorkflow.GetSectionStatusesAsync(establishmentId, sectionIds);
    }

    public Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId)
    {
        return _submissionWorkflow.SetLatestSubmissionViewedAsync(establishmentId, sectionId);
    }

    public Task<int> SubmitAnswerAsync(int userId, int establishmentId, SubmitAnswerModel answerModel)
    {
        return _submissionWorkflow.SubmitAnswer(userId, establishmentId, answerModel);
    }

    public Task ConfirmCheckAnswersAsync(int submissionId)
    {
        return _submissionWorkflow.SetMaturityAndMarkAsReviewedAsync(submissionId);
    }

    public async Task DeleteCurrentSubmissionHardAsync(int establishmentId, string sectionId)
    {
        await _submissionWorkflow.DeleteSubmissionHardAsync(establishmentId, sectionId);
    }

    public async Task DeleteCurrentSubmissionSoftAsync(int establishmentId, string sectionId)
    {
        await _submissionWorkflow.DeleteSubmissionSoftAsync(establishmentId, sectionId);
    }
}

using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Core.RoutingDataModels;

namespace Dfe.PlanTech.Application.Services;

public class SubmissionService(
    ContentfulWorkflow contentfulWorkflow,
    ResponseWorkflow responseWorkflow,
    SubmissionWorkflow submissionWorkflow
)
{
    private readonly ContentfulWorkflow _contentfulWorkflow = contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));
    private readonly ResponseWorkflow _responseWorkflow = responseWorkflow ?? throw new ArgumentNullException(nameof(responseWorkflow));
    private readonly SubmissionWorkflow _submissionWorkflow = submissionWorkflow ?? throw new ArgumentNullException(nameof(submissionWorkflow));

    public async Task RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(int establishmentId, string sectionId)
    {
        // Check if an in-progress submission already exists
        var inProgressSubmission = await _submissionWorkflow.GetInProgressSubmissionAsync(establishmentId, sectionId);
        if (inProgressSubmission != null)
        {
            await _submissionWorkflow.SetSubmissionInaccessibleAsync(inProgressSubmission.Id);
        }

        await _submissionWorkflow.CloneLatestCompletedSubmission(establishmentId, sectionId);
    }

    public async Task<SubmissionResponsesModel?> GetLatestSubmissionWithResponsesAsync(int establishmentId, string sectionSlug, bool isCompletedSubmission)
    {
        var cmsQuestionnaireSection = await _contentfulWorkflow.GetSectionBySlugAsync(sectionSlug);

        return await _responseWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, cmsQuestionnaireSection, isCompletedSubmission);
    }

    public async Task<SubmissionRoutingDataModel> GetSubmissionRoutingDataAsync(int establishmentId, string sectionSlug)
    {
        var cmsQuestionnaireSection = await _contentfulWorkflow.GetSectionBySlugAsync(sectionSlug);

        var latestCompletedSubmission = await _submissionWorkflow.GetLatestSubmissionAsync(
            establishmentId,
            cmsQuestionnaireSection.Id,
            isCompletedSubmission: true,
            includeResponses: false);

        var status = latestCompletedSubmission is null
            ? SubmissionStatus.NotStarted
            : latestCompletedSubmission.Completed
                ? SubmissionStatus.CompleteReviewed
                : SubmissionStatus.InProgress;

        if (status.Equals(SubmissionStatus.NotStarted) || status.Equals(SubmissionStatus.CompleteReviewed))
        {
            return new SubmissionRoutingDataModel
            {
                NextQuestion = cmsQuestionnaireSection.Questions.First(),
                QuestionnaireSection = cmsQuestionnaireSection,
                Status = status
            };
        }

        var latestCompletedSubmissionResponses = await _responseWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(
            establishmentId,
            cmsQuestionnaireSection,
            isCompletedSubmission: false
        );
        if (latestCompletedSubmissionResponses is null)
        {
            throw new InvalidDataException($"No incomplete responses found for section with ID {cmsQuestionnaireSection.Id}.");
        }

        var lastResponse = latestCompletedSubmissionResponses.Responses.Last();
        var cmsLastAnswer = cmsQuestionnaireSection.Questions
            .FirstOrDefault(q => q.Id.Equals(lastResponse.QuestionSysId))?
            .Answers
            .FirstOrDefault(a => a.Id.Equals(lastResponse.AnswerSysId));

        return new SubmissionRoutingDataModel
        {
            NextQuestion = cmsLastAnswer?.NextQuestion ?? cmsQuestionnaireSection.Questions.First(),
            QuestionnaireSection = cmsQuestionnaireSection,
            Submission = latestCompletedSubmissionResponses,
            Status = cmsLastAnswer?.NextQuestion is null
                ? SubmissionStatus.CompleteNotReviewed
                : SubmissionStatus.InProgress
        };
    }

    public Task ConfirmCheckAnswers(int submissionId)
    {
        return _submissionWorkflow.SetMaturityAndMarkAsReviewedAsync(submissionId);
    }

    public async Task DeleteCurrentSubmission(int establishmentId, string sectionId)
    {
        await _submissionWorkflow.SetSubmissionInaccessibleAsync(establishmentId, sectionId);
    }
}

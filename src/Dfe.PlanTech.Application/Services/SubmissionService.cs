using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Enums;
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

    public async Task<SubmissionRoutingDataModel> GetSubmissionRoutingDataAsync(int establishmentId, string sectionSlug)
    {
        var cmsQuestionnaireSection = await _contentfulWorkflow.GetSectionBySlugAsync(sectionSlug);

        var latestCompletedSubmission = await _responseWorkflow.GetLatestSubmission(
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

        latestCompletedSubmission = await _responseWorkflow.GetLatestIncompleteSubmissionWithOrderedResponses(establishmentId, cmsQuestionnaireSection);
        if (latestCompletedSubmission is null)
        {
            throw new InvalidDataException($"No incomplete responses found for section with ID {cmsQuestionnaireSection.Id}.");
        }

        var lastResponse = latestCompletedSubmission.Responses.Last();
        var cmsLastAnswer = cmsQuestionnaireSection.Questions
            .FirstOrDefault(q => q.Id.Equals(lastResponse.Question.ContentfulSysId))?
            .Answers
            .FirstOrDefault(a => a.Id.Equals(lastResponse.Answer.ContentfulSysId));

        return new SubmissionRoutingDataModel
        {
            NextQuestion = cmsLastAnswer?.NextQuestion ?? cmsQuestionnaireSection.Questions.First(),
            QuestionnaireSection = cmsQuestionnaireSection,
            Submission = latestCompletedSubmission,
            Status = cmsLastAnswer?.NextQuestion is null
                ? SubmissionStatus.CompleteNotReviewed
                : SubmissionStatus.InProgress
        };
    }

    public async Task<string> GetSubtopicRecommendationIntroSlug(int establishmentId, string sectionSlug)
    {
        var cmsQuestionnaireSection = await _contentfulWorkflow.GetSectionBySlugAsync(sectionSlug);

        var latestCompletedSubmission = await _responseWorkflow.GetLatestSubmission(
           establishmentId,
           cmsQuestionnaireSection.Id,
           isCompletedSubmission: true,
           includeResponses: false);

        if (latestCompletedSubmission is null)
        {
            throw new InvalidDataException($"No incomplete responses found for section with ID {cmsQuestionnaireSection.Id}.");
        }

        if (latestCompletedSubmission.Maturity is null)
        {
            throw new InvalidDataException($"No maturity recorded for submission with ID {latestCompletedSubmission.Id}.");
        }

        var maturity = latestCompletedSubmission.Maturity;
        var introSlugForMaturity = await _contentfulWorkflow.GetIntroForMaturityAsync(cmsQuestionnaireSection.Id, maturity);
        if (introSlugForMaturity is null)
        {
            throw new InvalidDataException($"No recommendation intro found maturity {maturity} for section with ID {cmsQuestionnaireSection.Id}.");
        }

        return introSlugForMaturity.Slug;
    }

    public async Task DeleteCurrentSubmission(int establishmentId, string sectionId)
    {
        await _submissionWorkflow.SetSubmissionInaccessibleAsync(establishmentId, sectionId);
    }
}

using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services;

public class QuestionService(
    SubmissionWorkflow submissionWorkflow
)
{
    private readonly SubmissionWorkflow _submissionWorkflow = submissionWorkflow ?? throw new ArgumentNullException(nameof(submissionWorkflow));

    public async Task<QuestionnaireQuestionEntry?> GetNextUnansweredQuestion(int establishmentId, QuestionnaireSectionEntry section)
    {
        var submission = await _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, isCompletedSubmission: false);
        if (submission is null)
            return section.Questions.FirstOrDefault();

        if (!submission.Responses.Any())
            throw new DatabaseException($"There are no responses in the database for ongoing submission '{submission.Id}' linked to establishment '{establishmentId}'");

        var submissionResponsesModel = new SubmissionResponsesModel(submission, section);

        return GetValidatedNextUnansweredQuestion(section, submissionResponsesModel);
    }

    /// <summary>
    /// Uses answered questions to find the next. If it is not possible to order user responses against the current questions,
    /// this indicates that content has changed or another user finished the submission concurrently.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="answeredQuestions"></param>
    /// <returns></returns>
    /// <exception cref="DatabaseException"></exception>
    private static QuestionnaireQuestionEntry? GetValidatedNextUnansweredQuestion(QuestionnaireSectionEntry section, SubmissionResponsesModel answeredQuestions)
    {
        var lastAttachedResponse = answeredQuestions.Responses.LastOrDefault();
        if (lastAttachedResponse is null)
            throw new DatabaseException($"The responses to the ongoing submission {answeredQuestions.SubmissionId} are out of sync with the topic");

        return section.Questions
            .Where(question => question.Sys.Id.Equals(lastAttachedResponse.QuestionSysId))
            .SelectMany(question => question.Answers)
            .Where(answer => answer.Sys.Id.Equals(lastAttachedResponse.AnswerSysId))
            .Select(answer => answer.NextQuestion)
            .FirstOrDefault();
    }
}

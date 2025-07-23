using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Application.Services;

public class QuestionService(
    ResponseWorkflow responseWorkflow
)
{
    private readonly ResponseWorkflow _responseWorkflow = responseWorkflow ?? throw new ArgumentNullException(nameof(responseWorkflow));

    public async Task<CmsQuestionnaireQuestionDto?> GetNextUnansweredQuestion(int establishmentId, CmsQuestionnaireSectionDto section)
    {
        var answeredQuestions = await _responseWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, isCompletedSubmission: false);
        if (answeredQuestions is null)
            return section.Questions.FirstOrDefault();

        if (answeredQuestions.Responses.Count == 0)
            throw new DatabaseException($"There are no responses in the database for ongoing submission {answeredQuestions.SubmissionId}, linked to establishment {establishmentId}");

        return GetValidatedNextUnansweredQuestion(section, answeredQuestions);
    }

    /// <summary>
    /// Uses answered questions to find the next. If it is not possible to order user responses against the current questions,
    /// this indicates that content has changed or another user finished the submission concurrently.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="answeredQuestions"></param>
    /// <returns></returns>
    /// <exception cref="DatabaseException"></exception>
    private static CmsQuestionnaireQuestionDto? GetValidatedNextUnansweredQuestion(CmsQuestionnaireSectionDto section, SubmissionResponsesModel answeredQuestions)
    {
        var lastAttachedResponse = answeredQuestions.Responses.LastOrDefault();
        if (lastAttachedResponse is null)
            throw new DatabaseException($"The responses to the ongoing submission {answeredQuestions.SubmissionId} are out of sync with the topic");

        return section.Questions
            .Where(question => question.Id.Equals(lastAttachedResponse.QuestionSysId))
            .SelectMany(question => question.Answers)
            .Where(answer => answer.Id.Equals(lastAttachedResponse.AnswerSysId))
            .Select(answer => answer.NextQuestion)
            .FirstOrDefault();
    }
}

using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

public class GetNextUnansweredQuestionQuery : IGetNextUnansweredQuestionQuery
{
    private readonly IGetLatestResponsesQuery _getResponseQuery;

    public GetNextUnansweredQuestionQuery(IGetLatestResponsesQuery getResponseQuery)
    {
        _getResponseQuery = getResponseQuery;
    }

    public async Task<Question?> GetNextUnansweredQuestion(int establishmentId, Section section, CancellationToken cancellationToken = default)
    {
        var answeredQuestions = await _getResponseQuery.GetLatestResponses(establishmentId, section.Sys.Id, false, cancellationToken);

        if (answeredQuestions == null) return section.Questions.FirstOrDefault();

        if (answeredQuestions.Responses.Count == 0) throw new DatabaseException($"There are no responses in the database for ongoing submission {answeredQuestions.SubmissionId}, linked to establishment {establishmentId}");

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
    private static Question? GetValidatedNextUnansweredQuestion(Section section, SubmissionResponsesDto answeredQuestions)
    {
        var lastAttachedResponse = section.GetOrderedResponsesForJourney(answeredQuestions.Responses).LastOrDefault();

        if (lastAttachedResponse == null)
            throw new DatabaseException($"The responses to the ongoing submission {answeredQuestions.SubmissionId} are out of sync with the topic");

        return section.Questions.Where(question => question.Sys.Id == lastAttachedResponse.QuestionRef)
                              .SelectMany(question => question.Answers)
                              .Where(answer => answer.Sys.Id == lastAttachedResponse.AnswerRef)
                              .Select(answer => answer.NextQuestion)
                              .FirstOrDefault();
    }
}

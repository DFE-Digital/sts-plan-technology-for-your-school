using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;

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
        var answeredQuestions = await _getResponseQuery.GetLatestResponses(establishmentId, section.Sys.Id, cancellationToken);

        if (answeredQuestions == null) return section.Questions.FirstOrDefault();

        if (answeredQuestions.Responses.Count == 0) throw new DatabaseException($"There are no responses in the database for ongoing submission {answeredQuestions.SubmissionId}, linked to establishment {establishmentId}");

        return GetValidatedNextUnansweredQuestion(section, answeredQuestions);
    }

    /// <summary>
    /// Uses answered questions to find the next. If it is not possible to link responses to questions,
    /// this indicates that a content change has occured, or another user has edited the response concurrently.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="answeredQuestions"></param>
    /// <returns></returns>
    /// <exception cref="DatabaseException"></exception>
    private static Question? GetValidatedNextUnansweredQuestion(Section section, CheckAnswerDto answeredQuestions)
    {
        var orderedResponses = section.GetOrderedResponsesForJourney(answeredQuestions.Responses).ToList();
        
        if (orderedResponses.Count == 0)
            throw new DatabaseException($"The responses to the ongoing submission {answeredQuestions.SubmissionId} are out of sync with the topic");

        var lastAttachedResponse = orderedResponses.Last(); 

        return section.Questions.Where(question => question.Sys.Id == lastAttachedResponse.QuestionRef)
                              .SelectMany(question => question.Answers)
                              .Where(answer => answer.Sys.Id == lastAttachedResponse.AnswerRef)
                              .Select(answer => answer.NextQuestion)
                              .FirstOrDefault();
    }
}

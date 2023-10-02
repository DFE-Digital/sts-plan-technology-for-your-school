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

    if (!answeredQuestions.Responses.Any()) throw new DatabaseException($"There are no responses in the database for ongoing submission {answeredQuestions.SubmissionId}, linked to establishment {establishmentId}");

    return GetNextUnansweredQuestion(section, answeredQuestions.Responses);
  }

  public static Question? GetNextUnansweredQuestion(Section section, List<QuestionWithAnswer> responses)
  {
    var lastAttachedResponse = section.GetAttachedQuestions(responses).Last();

    return section.Questions.Where(question => question.Sys.Id == lastAttachedResponse.QuestionRef)
                          .SelectMany(question => question.Answers)
                          .Where(answer => answer.Sys.Id == lastAttachedResponse.AnswerRef)
                          .Select(answer => answer.NextQuestion)
                          .FirstOrDefault();
  }
}

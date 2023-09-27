using Dfe.PlanTech.Application.Questionnaire.Interfaces;
using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Domain.Questionnaire.Models;

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

    if (answeredQuestions == null || answeredQuestions.Responses.Count == 0) return section.Questions.FirstOrDefault();

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

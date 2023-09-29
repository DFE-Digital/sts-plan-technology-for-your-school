using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interface;

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
        Question? node = section.Questions[0];

        while (node != null)
        {
            var response = responses.Find(response => response.QuestionRef == node.Sys.Id);
            if (response != null)
            {
                var answer = Array.Find(node.Answers, answer => answer.Sys.Id == response.AnswerRef);

                response = response with
                {
                    AnswerText = answer?.Text ?? response.AnswerText,
                    QuestionText = node.Text,
                    QuestionSlug = node.Slug
                };

                node = Array.Find(node.Answers, answer => answer.Sys.Id.Equals(response.AnswerRef))?.NextQuestion;
            }
            else
            {
                return node;
            }
        }

        return null;
    }
}

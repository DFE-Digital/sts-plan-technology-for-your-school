using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

/// <summary>
/// A sub-section of a <see chref="Category"/>
/// </summary>
public class Section : ContentComponent, ISectionContentComponent
{
    public string Name { get; init; } = null!;

    public List<Question> Questions { get; init; } = new();

    public string FirstQuestionId => Questions.Select(question => question.Sys.Id).FirstOrDefault() ?? "";

    public Page InterstitialPage { get; init; } = null!;

    public List<RecommendationPage> Recommendations { get; init; } = new();

    public RecommendationPage? TryGetRecommendationForMaturity(Maturity maturity) => Recommendations.Find(recommendation => recommendation.Maturity == maturity);

    public RecommendationPage? GetRecommendationForMaturity(string? maturity)
    {
        if (string.IsNullOrEmpty(maturity) || !Enum.TryParse(maturity, out Maturity maturityResponse)) return null;

        return TryGetRecommendationForMaturity(maturityResponse);
    }

    public IEnumerable<QuestionWithAnswer> GetAttachedQuestions(IEnumerable<QuestionWithAnswer> responses)
    {
        var questionWithAnswerMap = responses.ToDictionary(questionWithAnswer => questionWithAnswer.QuestionRef,
                                                                                 questionWithAnswer => questionWithAnswer);

        Question? node = Questions[0];

        while (node != null)
        {
            if (!questionWithAnswerMap.TryGetValue(node.Sys.Id, out QuestionWithAnswer? questionWithAnswer))
            {
                break;
            }
            Answer? answer = GetAnswerForRef(node, questionWithAnswer);

            questionWithAnswer = questionWithAnswer with
            {
                AnswerText = answer?.Text ?? questionWithAnswer.AnswerText,
                QuestionText = node.Text,
                QuestionSlug = node.Slug
            };

            yield return questionWithAnswer;
            node = GetAnswerForRef(node, questionWithAnswer)?.NextQuestion;
        }
    }

    private static Answer? GetAnswerForRef(Question node, QuestionWithAnswer questionWithAnswer)
    => node.Answers.Find(answer => answer.Sys.Id.Equals(questionWithAnswer.AnswerRef));
}

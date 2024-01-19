using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

/// <summary>
/// A sub-section of a <see chref="Category"/>
/// </summary>
public class Section : ContentComponent, ISectionComponent
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

        Question? node = Questions.FirstOrDefault();

        while (node != null)
        {
            if (!questionWithAnswerMap.TryGetValue(node.Sys.Id, out QuestionWithAnswer? questionWithAnswer))
            {
                break;
            }

            Answer? answer = GetAnswerForRef(questionWithAnswer);
            var question = Questions.Find(q => q.Sys.Id == questionWithAnswer.QuestionRef);

            var answerrr = question?.Answers.Find(answer => answer.Sys.Id == questionWithAnswer.AnswerRef);
            questionWithAnswer = questionWithAnswer with
            {
                AnswerText = answer?.Text ?? questionWithAnswer.AnswerText,
                QuestionText = node.Text,
                QuestionSlug = node.Slug
            };

            yield return questionWithAnswer;

            //This ensures matching behaviour to current functionality.
            var nextQuestion = answer?.NextQuestion != null ? Questions.Find(question => question.Sys.Id == answer.NextQuestion.Sys.Id) : null;
            node = nextQuestion;//answer?.NextQuestion;
        }
    }

    private Answer? GetAnswerForRef(QuestionWithAnswer questionWithAnswer)
        => Questions.Find(q => q.Sys.Id == questionWithAnswer.QuestionRef)?
                    .Answers.Find(answer => answer.Sys.Id == questionWithAnswer.AnswerRef);
}

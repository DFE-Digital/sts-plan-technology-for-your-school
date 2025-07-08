using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

/// <summary>
/// A sub-section of a <see chref="Category"/>
/// </summary>
public class Section : ContentComponent, ISectionComponent
{
    public string Name { get; init; } = null!;
    public List<QuestionEntry> Questions { get; init; } = new();

    public string FirstQuestionId => Questions.Select(question => question.Sys.Id).FirstOrDefault() ?? "";

    public Page? InterstitialPage { get; init; }

    public IEnumerable<QuestionWithAnswer> GetOrderedResponsesForJourney(IEnumerable<QuestionWithAnswer> responses)
    {
        var questionWithAnswerMap = responses
                    .GroupBy(r => r.QuestionRef)
                    .ToDictionary(g => g.Key, g => g.First());

        QuestionEntry? node = Questions.FirstOrDefault();

        while (node != null)
        {
            if (!questionWithAnswerMap.TryGetValue(node.Sys.Id, out QuestionWithAnswer? questionWithAnswer))
                break;

            QuestionnaireAnswerEntry? answer = GetAnswerForRef(questionWithAnswer);

            // Show the latest Text and Slug, but preserve user answer if there has been a change
            questionWithAnswer = questionWithAnswer with
            {
                AnswerText = answer?.Text ?? questionWithAnswer.AnswerText,
                QuestionText = node.Text,
                QuestionSlug = node.Slug
            };

            yield return questionWithAnswer;

            node = answer?.NextQuestion;
        }
    }

    private QuestionnaireAnswerEntry? GetAnswerForRef(QuestionWithAnswer questionWithAnswer)
        => Questions.Find(q => q.Sys.Id == questionWithAnswer.QuestionRef)?.Answers.Find(answer => answer.Sys.Id == questionWithAnswer.AnswerRef);
}

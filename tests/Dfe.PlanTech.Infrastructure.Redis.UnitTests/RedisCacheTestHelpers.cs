using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;

public static class RedisCacheTestHelpers
{
    private static readonly QuestionnaireAnswerEntry _firstAnswer = new()
    {
        Sys = new SystemDetails { Id = "answer-one-id" },
        Text = "answer-one-text",
    };
    private static readonly QuestionnaireAnswerEntry _secondAnswer = new()
    {
        Sys = new SystemDetails { Id = "answer-two-id" },
        Text = "answer-two-text",
    };
    private static readonly QuestionnaireQuestionEntry _question = new()
    {
        Sys = new SystemDetails { Id = "question-one-id" },
        Slug = "question-one-slug",
        Text = "question-one-text",
        Answers = [_firstAnswer, _secondAnswer],
    };

    public static QuestionnaireAnswerEntry FirstAnswer => _firstAnswer;
    public static QuestionnaireAnswerEntry SecondAnswer => _secondAnswer;
    public static QuestionnaireQuestionEntry Question => _question;
    public static List<QuestionnaireQuestionEntry> EmptyQuestionCollection => [];
}

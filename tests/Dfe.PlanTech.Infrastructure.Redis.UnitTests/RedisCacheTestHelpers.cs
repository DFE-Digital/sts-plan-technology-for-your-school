using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;

public static class RedisCacheTestHelpers
{
    private static readonly Answer _firstAnswer = new()
    {
        Sys = new SystemDetails { Id = "answer-one-id" },
        Maturity = "high",
        Text = "answer-one-text",
    };
    private static readonly Answer _secondAnswer = new()
    {
        Sys = new SystemDetails { Id = "answer-two-id" },
        Maturity = "medium",
        Text = "answer-two-text",
    };
    private static readonly Question _question = new()
    {
        Sys = new SystemDetails { Id = "question-one-id" },
        Slug = "question-one-slug",
        Text = "question-one-text",
        Answers = [_firstAnswer, _secondAnswer]
    };

    public static Answer FirstAnswer => _firstAnswer;
    public static Answer SecondAnswer => _secondAnswer;
    public static Question Question => _question;
    public static List<Question> EmptyQuestionCollection => [];
}

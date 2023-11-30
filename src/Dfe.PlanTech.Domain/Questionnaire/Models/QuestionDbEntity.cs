using Dfe.PlanTech.Domain.Database.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class QuestionDbEntity : IQuestion<AnswerDbEntity>, IDbEntity
{
    public long Id { get; set; }

    public string ContentfulId { get; set; } = null!;

    public string Text { get; init; } = null!;

    public string? HelpText { get; init; }

    public AnswerDbEntity[] Answers { get; init; } = Array.Empty<AnswerDbEntity>();

    public string Param { get; init; } = null!;

    public string Slug { get; set; } = null!;

    public AnswerDbEntity[] PreviousAnswers { get; init; } = Array.Empty<AnswerDbEntity>();
}

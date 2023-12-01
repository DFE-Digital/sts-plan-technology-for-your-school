using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class QuestionDbEntity : ContentComponentDbEntity, IQuestion<AnswerDbEntity>
{
    public string Text { get; init; } = null!;

    public string? HelpText { get; init; }

    public AnswerDbEntity[] Answers { get; init; } = Array.Empty<AnswerDbEntity>();

    public string Param { get; init; } = null!;

    public string Slug { get; set; } = null!;
}

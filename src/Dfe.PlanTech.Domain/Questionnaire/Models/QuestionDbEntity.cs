using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class QuestionDbEntity : ContentComponentDbEntity, IQuestion<AnswerDbEntity>
{
    public string Text { get; init; } = null!;

    public string? HelpText { get; init; }

    public List<AnswerDbEntity> Answers { get; init; } = new();

    public string Slug { get; set; } = null!;

    public List<AnswerDbEntity> PreviousAnswers { get; init; } = new();
}

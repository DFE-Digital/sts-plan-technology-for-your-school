using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class AnswerDbEntity : ContentComponentDbEntity, IAnswer<QuestionDbEntity>
{
    public string Text { get; set; } = null!;

    public string? NextQuestionId { get; set; }

    public QuestionDbEntity? NextQuestion { get; set; }

    public string Maturity { get; set; } = null!;

    [DontCopyValue]
    public string? ParentQuestionId { get; set; }

    public QuestionDbEntity? ParentQuestion { get; set; }
}

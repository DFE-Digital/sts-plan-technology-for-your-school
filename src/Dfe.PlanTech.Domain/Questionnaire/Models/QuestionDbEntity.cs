using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class QuestionDbEntity : ContentComponentDbEntity, IQuestion<AnswerDbEntity>, IHasSlug
{
    public string Text { get; set; } = null!;

    public string? HelpText { get; set; }

    public List<AnswerDbEntity> Answers { get; set; } = new();

    public string Slug { get; set; } = null!;

    public List<AnswerDbEntity> PreviousAnswers { get; set; } = new();

    public SectionDbEntity? Section { get; set; }

    [DontCopyValue]
    public string? SectionId { get; set; }
}

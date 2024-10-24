using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class QuestionDbEntity : ContentComponentDbEntity, IQuestion<AnswerDbEntity>, IHasSlug
{
    public string Text { get; set; } = null!;

    public string? HelpText { get; set; }

    [DontCopyValue]
    public List<AnswerDbEntity> Answers { get; set; } = [];

    public string Slug { get; set; } = null!;

    [DontCopyValue]
    public SectionDbEntity? Section { get; set; }

    [DontCopyValue]
    public string? SectionId { get; set; }
}

using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class QuestionnaireAnswerEntry: TransformableEntry<QuestionnaireAnswerEntry, CmsQuestionnaireAnswerDto>
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public QuestionnaireQuestionEntry? NextQuestion { get; init; }
    public string Maturity { get; init; } = null!;

    public QuestionnaireAnswerEntry() : base(entry => new CmsQuestionnaireAnswerDto(entry)) { }
}

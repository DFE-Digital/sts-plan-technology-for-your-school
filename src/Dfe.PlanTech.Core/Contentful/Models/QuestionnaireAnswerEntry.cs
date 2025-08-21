namespace Dfe.PlanTech.Core.Contentful.Models;

public class QuestionnaireAnswerEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public QuestionnaireQuestionEntry? NextQuestion { get; init; }
    public string Maturity { get; init; } = null!;
}

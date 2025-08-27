using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class QuestionnaireQuestionEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public string? HelpText { get; init; }
    public IEnumerable<QuestionnaireAnswerEntry> Answers { get; set; } = [];
    public string Slug { get; set; } = null!;
}

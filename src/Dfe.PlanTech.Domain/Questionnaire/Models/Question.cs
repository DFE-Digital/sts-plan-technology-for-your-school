using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Question : ContentComponent
{
    /// <summary>
    /// Actual question text
    /// </summary>
    public string Text { get; init; } = null!;

    /// <summary>
    /// Optional help text
    /// </summary>
    public string? HelpText { get; init; }

    public Answer[] Answers { get; init; } = Array.Empty<Answer>();
}

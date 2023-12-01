using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Question : ContentComponent, IQuestion<Answer>
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

    public string Slug { get; set; } = null!;
}

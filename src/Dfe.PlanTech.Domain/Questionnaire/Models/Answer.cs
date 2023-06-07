using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Answer : ContentComponent
{
    /// <summary>
    /// Actual answer
    /// </summary>
    /// <value></value>
    public string Text { get; init; } = null!;
}

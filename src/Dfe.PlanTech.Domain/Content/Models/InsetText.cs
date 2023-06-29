using Dfe.PlanTech.Domain.Content.Enums;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for Inset Text component type
/// </summary>
public class InsetText : ContentComponent
{
    /// <summary>
    /// The body of the component
    /// </summary>
    public TextBody Text { get; init; } = null!;
}

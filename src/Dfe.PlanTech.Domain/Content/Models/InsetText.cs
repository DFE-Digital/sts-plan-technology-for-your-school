namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for Inset Text component type
/// </summary>
public class InsetText : ContentComponent
{
    /// <summary>
    /// The body of the component
    /// </summary>
    public string Text { get; init; } = null!;
}

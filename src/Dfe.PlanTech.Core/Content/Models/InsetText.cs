using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Core.Content.Models;

/// <summary>
/// Model for Inset Text component type
/// </summary>
/// <inheritdoc/>
public class InsetText : ContentComponent, IInsetText
{
    public string Text { get; init; } = null!;
}

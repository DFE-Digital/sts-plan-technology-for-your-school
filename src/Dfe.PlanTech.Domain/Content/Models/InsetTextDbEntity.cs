using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for database table for the Inset Text component type
/// </summary>
/// <inheritdoc/>
public class InsetTextDbEntity : ContentComponentDbEntity, IInsetText
{
    public string? Text { get; init; }
}

using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Core.Content.Models;

/// <summary>
/// Model for Title content type
/// </summary>
public class Title : ContentComponent, ITitle
{
    public string Text { get; init; } = null!;
}

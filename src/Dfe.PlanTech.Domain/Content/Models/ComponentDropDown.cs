using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for DropDown type.
/// </summary>
/// <inheritdoc/>
public class ComponentDropDown : ContentComponent, IComponentDropDown<RichTextContent>
{
    public string Title { get; init; } = null!;

    public RichTextContent RichTextContent { get; init; } = null!;
}

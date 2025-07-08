using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Core.Content.Models;

/// <summary>
/// Model for DropDown type.
/// </summary>
/// <inheritdoc/>
public class DropDownEntry : ContentComponent, IComponentDropDown<RichTextContent>
{
    public string InternalName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public RichTextContent? Content { get; set; } = null!;
}

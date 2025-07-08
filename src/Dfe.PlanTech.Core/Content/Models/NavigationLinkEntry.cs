namespace Dfe.PlanTech.Core.Content.Models;

public class NavigationLinkEntry : ContentComponent
{
    public string InternalName { get; set; } = null!;
    public string DisplayText { get; set; } = null!;
    public string? Href { get; set; } = null;
    public bool OpenInNewTab { get; set; } = false;

    // TODO: Move to DTO
    public ContentComponent? ContentToLinkTo { get; set; }
}

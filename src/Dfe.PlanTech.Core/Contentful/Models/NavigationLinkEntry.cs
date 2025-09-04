namespace Dfe.PlanTech.Core.Contentful.Models;

public class NavigationLinkEntry : ContentfulEntry
{
    public ContentfulEntry? ContentToLinkTo { get; set; }
    public string InternalName { get; set; } = null!;
    public string DisplayText { get; set; } = null!;
    public string? Href { get; set; } = null;
    public bool OpenInNewTab { get; set; } = false;

    public bool IsValid => !string.IsNullOrEmpty(DisplayText) && (!string.IsNullOrEmpty(Href) || ContentToLinkTo != null);
}

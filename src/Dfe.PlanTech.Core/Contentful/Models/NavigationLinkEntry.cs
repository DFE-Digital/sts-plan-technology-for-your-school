using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class NavigationLinkEntry : TransformableEntry<NavigationLinkEntry, CmsNavigationLinkDto>
{
    public ContentComponent? ContentToLinkTo { get; set; }
    public string InternalName { get; set; } = null!;
    public string DisplayText { get; set; } = null!;
    public string? Href { get; set; } = null;
    public bool OpenInNewTab { get; set; } = false;

    protected override Func<NavigationLinkEntry, CmsNavigationLinkDto> Constructor => entry => new(entry);
}

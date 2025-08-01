using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class NavigationLinkEntry : TransformableEntry<NavigationLinkEntry, CmsNavigationLinkDto>, IContentfulEntry
{
    public Entry<ContentComponent>? ContentToLinkTo { get; set; }
    public string InternalName { get; set; } = null!;
    public string DisplayText { get; set; } = null!;
    public string? Href { get; set; } = null;
    public bool OpenInNewTab { get; set; } = false;

    public NavigationLinkEntry() : base(entry => new CmsNavigationLinkDto(entry)) { }
}

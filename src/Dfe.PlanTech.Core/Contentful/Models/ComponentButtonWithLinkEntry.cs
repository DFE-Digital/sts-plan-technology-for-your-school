using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonWithLinkEntry: TransformableEntry<ComponentButtonWithLinkEntry, CmsComponentButtonWithLinkDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public ComponentButtonEntry Button { get; init; } = null!;
    public string Href { get; init; } = null!;

    public ComponentButtonWithLinkEntry() : base(entry => new CmsComponentButtonWithLinkDto(entry)) { }
}

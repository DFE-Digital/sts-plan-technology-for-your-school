using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonWithLinkEntry: TransformableEntry<ComponentButtonWithLinkEntry, CmsComponentButtonWithLinkDto>
{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = null!;
    public ComponentButtonEntry Button { get; init; } = null!;
    public string Href { get; init; } = null!;

    public ComponentButtonWithLinkEntry() : base(entry => new CmsComponentButtonWithLinkDto(entry)) { }
}

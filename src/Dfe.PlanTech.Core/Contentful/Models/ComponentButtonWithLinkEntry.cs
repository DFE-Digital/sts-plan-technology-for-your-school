using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonWithLinkEntry: TransformableEntry<ComponentButtonWithLinkEntry, CmsComponentButtonWithLinkDto>
{
    public string InternalName { get; set; } = null!;
    public ComponentButtonEntry Button { get; init; } = null!;
    public string Href { get; init; } = null!;

    protected override Func<ComponentButtonWithLinkEntry, CmsComponentButtonWithLinkDto> Constructor => entry => new(entry);
}

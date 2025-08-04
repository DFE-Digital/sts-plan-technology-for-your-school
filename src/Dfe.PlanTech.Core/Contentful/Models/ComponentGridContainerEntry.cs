using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentGridContainerEntry: TransformableEntry<ComponentGridContainerEntry, CmsComponentGridContainerDto>
{
    public string? InternalName { get; set; }
    public IEnumerable<ComponentCardEntry>? Content { get; set; }

    protected override Func<ComponentGridContainerEntry, CmsComponentGridContainerDto> Constructor => entry => new(entry);
}

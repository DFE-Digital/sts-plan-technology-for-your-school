using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentGridContainerEntry: TransformableEntry<ComponentGridContainerEntry, CmsComponentGridContainerDto>
{
    public string? InternalName { get; set; }
    public IEnumerable<ComponentCardEntry>? Content { get; set; }

    public ComponentGridContainerEntry() : base(entry => new CmsComponentGridContainerDto(entry)) { }
}

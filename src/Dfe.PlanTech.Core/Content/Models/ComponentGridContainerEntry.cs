using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentGridContainerEntry : Entry<ContentComponent>
{
    public string? InternalName { get; set; }
    public IReadOnlyList<ComponentCardEntry>? Content { get; set; }
}

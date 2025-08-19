namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentGridContainerEntry: ContentfulEntry
{
    public string? InternalName { get; set; }
    public IReadOnlyList<ComponentCardEntry>? Content { get; set; }
}

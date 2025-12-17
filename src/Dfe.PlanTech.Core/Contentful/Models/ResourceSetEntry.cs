namespace Dfe.PlanTech.Core.Contentful.Models;

public class ResourceSetEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Key { get; set; } = null!;
    public IEnumerable<ResourceEntry> Resources { get; set; } = [];
}

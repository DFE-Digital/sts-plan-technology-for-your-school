namespace Dfe.PlanTech.Core.Contentful.Models;

public class ResourceEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Value { get; init; } = null!;
    public List<string> Variables { get; init; } = [];
}

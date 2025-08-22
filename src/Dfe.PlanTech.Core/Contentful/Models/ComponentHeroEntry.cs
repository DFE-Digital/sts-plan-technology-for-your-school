namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentHeroEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public IEnumerable<ContentfulEntry> Content { get; set; } = null!;
}

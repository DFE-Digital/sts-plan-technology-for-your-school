namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentButtonWithEntryReferenceEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public ComponentButtonEntry Button { get; init; } = null!;
    public ContentfulEntry LinkToEntry { get; init; } = null!;
}

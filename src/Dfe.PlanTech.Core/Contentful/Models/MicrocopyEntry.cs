namespace Dfe.PlanTech.Core.Contentful.Models;

public class MicrocopyEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Value { get; init; } = null!;
}

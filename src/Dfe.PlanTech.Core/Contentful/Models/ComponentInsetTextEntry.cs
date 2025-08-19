namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentInsetTextEntry: ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
}

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentTextBodyEntry: ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public RichTextContentField RichText { get; init; } = null!;
}

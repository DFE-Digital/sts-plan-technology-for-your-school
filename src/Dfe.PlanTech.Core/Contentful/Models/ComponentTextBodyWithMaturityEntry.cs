namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentTextBodyWithMaturityEntry: ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public RichTextContentField TextBody { get; init; } = null!;
    public string Maturity { get; set; } = null!;
}

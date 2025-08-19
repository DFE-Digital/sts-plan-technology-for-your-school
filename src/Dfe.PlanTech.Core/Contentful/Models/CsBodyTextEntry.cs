namespace Dfe.PlanTech.Core.Contentful.Models;

public class CsBodyTextEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public RichTextContentField RichText { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Subtitle { get; set; } = null!;
}

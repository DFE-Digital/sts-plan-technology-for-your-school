namespace Dfe.PlanTech.Core.Contentful.Models;

public class RichTextContentSupportDataField : ContentfulField
{
    public string? Uri { get; init; }
    public RichTextContentDataEntry? Target { get; init; }
}

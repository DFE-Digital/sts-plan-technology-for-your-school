namespace Dfe.PlanTech.Core.Content.Models;

/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
/// <inheritdoc/>
public class RichTextContent : ContentComponent
{
    public string Value { get; set; } = "";

    public string NodeType { get; set; } = "";

    public List<RichTextMark> Marks { get; set; } = [];

    public List<RichTextContent> Content { get; set; } = [];

    public RichTextContentSupportData? Data { get; set; }
}

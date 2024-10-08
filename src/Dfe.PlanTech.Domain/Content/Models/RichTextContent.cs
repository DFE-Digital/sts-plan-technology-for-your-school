using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
/// <inheritdoc/>
public class RichTextContent : ContentComponent, IRichTextContent<RichTextMark, RichTextContent, RichTextData>
{
    public string Value { get; set; } = "";

    public string NodeType { get; set; } = "";

    public List<RichTextMark> Marks { get; set; } = [];

    public List<RichTextContent> Content { get; set; } = [];

    public RichTextData? Data { get; set; }
}

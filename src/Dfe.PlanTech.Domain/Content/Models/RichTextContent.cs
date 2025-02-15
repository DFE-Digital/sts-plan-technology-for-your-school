using Contentful.Core.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
/// <inheritdoc/>
public class RichTextContent : Entry<RichTextContent>, IContentComponent, IRichTextContent<RichTextMark, RichTextContent, CustomData>
{
    public string Value { get; set; } = "";

    public string NodeType { get; set; } = "";

    public List<RichTextMark> Marks { get; set; } = [];

    public List<RichTextContent> Content { get; set; } = [];

    public CustomData? Data { get; set; }
    public SystemDetails Sys { get; set; } = null!;
}

using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
/// <inheritdoc/>
public class RichTextContent : TransformableEntry<RichTextContent, CmsRichTextContentDto>
{
    public string Value { get; set; } = "";
    public string NodeType { get; set; } = "";
    public List<RichTextMarkEntry> Marks { get; set; } = [];
    public List<RichTextContent> Content { get; set; } = [];
    public RichTextContentSupportDataEntry? Data { get; set; }

    public RichTextContent() : base(entry => new CmsRichTextContentDto(entry)) { }
}

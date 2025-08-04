using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
/// <inheritdoc/>
public class RichTextContentEntry : TransformableEntry<RichTextContentEntry, CmsRichTextContentDto>
{
    public string Value { get; set; } = "";
    public string NodeType { get; set; } = "";
    public List<RichTextMarkEntry> Marks { get; set; } = [];
    public List<RichTextContentEntry> Content { get; set; } = [];
    public RichTextContentSupportDataEntry? Data { get; set; }


    protected override Func<RichTextContentEntry, CmsRichTextContentDto> Constructor => entry => new(entry);
}

using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
/// <inheritdoc/>
public class RichTextContentField : TransformableField<RichTextContentField, CmsRichTextContentDto>
{
    public string Value { get; set; } = "";
    public string NodeType { get; set; } = "";
    public List<RichTextMarkField> Marks { get; set; } = [];
    public List<RichTextContentField> Content { get; set; } = [];
    public RichTextContentSupportDataField? Data { get; set; }


    protected override Func<RichTextContentField, CmsRichTextContentDto> Constructor => entry => new(entry);
}

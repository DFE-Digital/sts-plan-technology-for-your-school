using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

/// <summary>
/// Mark type for the rich text (e.g. bold, underline)
/// </summary>
/// <inheritdoc/>
public class RichTextMark : TransformableEntry<RichTextMark, CmsRichTextMarkDto>
{
    public string Type { get; set; } = "";

    public RichTextMark() : base(entry => new CmsRichTextMarkDto(entry)) { }
}

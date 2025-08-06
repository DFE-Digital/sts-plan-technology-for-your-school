using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

/// <summary>
/// Mark type for the rich text (e.g. bold, underline)
/// </summary>
/// <inheritdoc/>
public class RichTextMarkField : TransformableField<RichTextMarkField, CmsRichTextMarkDto>
{
    public string Type { get; set; } = "";

    protected override Func<RichTextMarkField, CmsRichTextMarkDto> Constructor => entry => new(entry);
}

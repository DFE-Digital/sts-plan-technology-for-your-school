using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

/// <summary>
/// Mark type for the rich text (e.g. bold, underline)
/// </summary>
/// <inheritdoc/>
public class RichTextMarkEntry : TransformableEntry<RichTextMarkEntry, CmsRichTextMarkDto>
{
    public string Type { get; set; } = "";

    public RichTextMarkEntry() : base(entry => new CmsRichTextMarkDto(entry)) { }
}

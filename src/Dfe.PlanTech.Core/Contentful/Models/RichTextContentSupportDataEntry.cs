using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RichTextContentSupportDataEntry: TransformableEntry<RichTextContentSupportDataEntry, CmsRichTextContentSupportDataDto>
{
    public string? Uri { get; init; }
    public RichTextContentDataEntry? Target { get; init; }

    protected override Func<RichTextContentSupportDataEntry, CmsRichTextContentSupportDataDto> Constructor => entry => new(entry);
}

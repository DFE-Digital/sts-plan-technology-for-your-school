using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsRichTextContentSupportDataDto : CmsFieldDto
{
    public string? Uri { get; set; } = null!;
    public CmsRichTextContentDataDto? Target { get; set; }

    public CmsRichTextContentSupportDataDto(RichTextContentSupportDataField contentSupportData)
    {
        Uri = contentSupportData.Uri;
        Target = contentSupportData.Target?.AsDto();
    }
}

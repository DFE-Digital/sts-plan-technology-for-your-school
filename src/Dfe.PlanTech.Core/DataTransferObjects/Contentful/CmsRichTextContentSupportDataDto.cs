using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsRichTextContentSupportDataDto : CmsEntryDto
    {
        public string? Uri { get; set; } = null!;
        public CmsRichTextContentDataDto? Target { get; set; }

        public CmsRichTextContentSupportDataDto(RichTextContentSupportDataEntry contentSupportData)
        {
            Uri = contentSupportData.Uri;
            Target = contentSupportData.Target?.AsDto();
        }
    }
}

using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsRichTextMarkDto : CmsFieldDto
{
    public string Type { get; set; } = null!;
    public MarkType MarkType => Enum.TryParse(Type, true, out MarkType markType) ? markType : MarkType.Unknown;

    public CmsRichTextMarkDto(RichTextMarkField richTextMark)
    {
        Type = richTextMark.Type;
    }
}

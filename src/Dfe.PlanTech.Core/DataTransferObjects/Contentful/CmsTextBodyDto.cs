using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsTextBodyDto : CmsEntryDto
{
    public CmsRichTextContentDto RichText { get; init; } = null!;

    public CmsTextBodyDto(TextBodyEntry entry)
    {
        RichText = entry.RichText.AsDto();
    }
}

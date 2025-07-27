using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsComponentTextBodyDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public CmsRichTextContentDto RichText { get; init; } = null!;

    public CmsComponentTextBodyDto(ComponentTextBodyEntry textBodyEntry)
    {
        Id = textBodyEntry.Id;
        InternalName = textBodyEntry.InternalName;
        RichText = textBodyEntry.RichText.AsDto();
    }
}

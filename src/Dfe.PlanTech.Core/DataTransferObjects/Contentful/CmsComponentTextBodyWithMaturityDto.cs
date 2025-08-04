using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsComponentTextBodyWithMaturityDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public CmsRichTextContentDto TextBody { get; set; } = null!;
    public string Maturity { get; set; } = null!;

    public CmsComponentTextBodyWithMaturityDto(ComponentTextBodyWithMaturityEntry textBodyWithMaturityEntry)
    {
        Id = textBodyWithMaturityEntry.Id;
        InternalName = textBodyWithMaturityEntry.InternalName;
        TextBody = textBodyWithMaturityEntry.TextBody.AsDto();
        Maturity = textBodyWithMaturityEntry.Maturity;
    }
}

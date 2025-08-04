using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsCsBodyTextDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public CmsRichTextContentDto RichText { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Subtitle { get; set; } = null!;

    public CmsCsBodyTextDto(CsBodyTextEntry bodyTextEntry)
    {
        Id = bodyTextEntry.Id;
        InternalName = bodyTextEntry.InternalName;
        RichText = bodyTextEntry.RichText.AsDto();
        Title = bodyTextEntry.Title;
        Subtitle = bodyTextEntry.Subtitle;
    }
}

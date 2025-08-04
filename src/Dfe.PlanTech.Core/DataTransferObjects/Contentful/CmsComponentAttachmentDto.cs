using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsComponentAttachmentDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public string? Title { get; set; } = null!;
    public Asset Asset { get; set; } = null!;

    public CmsComponentAttachmentDto(ComponentAttachmentEntry attachmentEntry)
    {
        Id = attachmentEntry.Id;
        InternalName = attachmentEntry.InternalName;
        Title = attachmentEntry.Title;
        Asset = attachmentEntry.Asset;
    }
}

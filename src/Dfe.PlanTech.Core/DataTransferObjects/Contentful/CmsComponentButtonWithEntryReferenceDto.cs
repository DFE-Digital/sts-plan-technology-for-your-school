using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsComponentButtonWithEntryReferenceDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public CmsComponentButtonDto? Button { get; init; } = null!;
    public CmsEntryDto LinkToEntry { get; init; } = null!;

    public CmsComponentButtonWithEntryReferenceDto(ComponentButtonWithEntryReferenceEntry button)
    {
        Id = button.Id;
        InternalName = button.InternalName;
        Button = button.Button?.AsDto();
        LinkToEntry = BuildContentDto(button.LinkToEntry);
    }
}

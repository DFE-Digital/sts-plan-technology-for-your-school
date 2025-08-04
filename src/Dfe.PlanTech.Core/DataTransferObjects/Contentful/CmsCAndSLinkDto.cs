using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsCAndSLinkDto: CmsEntryDto
{
    public string InternalName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string LinkText { get; set; } = null!;

    public CmsCAndSLinkDto(CAndSLinkEntry linkEntry)
    {
        InternalName = linkEntry.InternalName;
        Url = linkEntry.Url;
        LinkText = linkEntry.LinkText;
    }
}

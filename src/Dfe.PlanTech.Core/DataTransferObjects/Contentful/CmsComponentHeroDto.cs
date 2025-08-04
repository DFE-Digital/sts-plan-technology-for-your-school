using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsComponentHeroDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public List<CmsEntryDto> Content { get; set; } = null!;

    public CmsComponentHeroDto(ComponentHeroEntry heroEntry)
    {
        Id = heroEntry.Id;
        InternalName = heroEntry.InternalName;
        Content = heroEntry.Content.Select(BuildContentDto).ToList();
    }
}

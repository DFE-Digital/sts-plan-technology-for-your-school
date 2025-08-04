using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsComponentButtonDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public string Value { get; init; } = null!;
    public bool IsStartButton { get; init; }

    public CmsComponentButtonDto(ComponentButtonEntry buttonEntry)
    {
        Id = buttonEntry.Id;
        InternalName = buttonEntry.InternalName;
        Value = buttonEntry.Value;
        IsStartButton = buttonEntry.IsStartButton;
    }
}

using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentJumpLinkEntry: TransformableEntry<ComponentJumpLinkEntry, CmsComponentJumpLinkDto>
{
    public string ComponentName { get; set; } = null!;
    public string JumpIdentifier { get; set; } = null!;

    protected override Func<ComponentJumpLinkEntry, CmsComponentJumpLinkDto> Constructor => entry => new(entry);
}

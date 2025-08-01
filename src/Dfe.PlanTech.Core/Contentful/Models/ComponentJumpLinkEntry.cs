using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentJumpLinkEntry: TransformableEntry<ComponentJumpLinkEntry, CmsComponentJumpLinkDto>, IContentfulEntry
{
    public string ComponentName { get; set; } = null!;
    public string JumpIdentifier { get; set; } = null!;

    public ComponentJumpLinkEntry() : base(entry => new CmsComponentJumpLinkDto(entry)) { }
}

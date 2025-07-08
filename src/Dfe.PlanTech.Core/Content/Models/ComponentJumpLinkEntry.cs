using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentJumpLinkEntry : Entry<ContentComponent>
{
    public string ComponentName { get; set; } = null!;
    public string JumpIdentifier { get; set; } = null!;
}

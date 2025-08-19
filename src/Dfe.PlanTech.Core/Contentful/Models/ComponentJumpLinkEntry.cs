namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentJumpLinkEntry : ContentfulEntry
{
    public string ComponentName { get; set; } = null!;
    public string JumpIdentifier { get; set; } = null!;
}

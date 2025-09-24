using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ComponentJumpLinkEntry : ContentfulEntry
{
    public string ComponentName { get; set; } = null!;
    public string JumpIdentifier { get; set; } = null!;
}

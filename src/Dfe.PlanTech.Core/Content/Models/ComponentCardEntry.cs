using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentCardEntry : Entry<ContentComponent>
{
    public string InternalName { get; init; } = null!;
    public string? Title { get; init; } = null!;
    public string? Description { get; init; } = null!;
    public string? Meta { get; init; } = null!;
    public Asset? Image { get; init; } = null!;
    public string? ImageAlt { get; init; } = null!;
    public string? Uri { get; init; } = null!;
}

using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentAttachmentEntry : Entry<ContentComponent>
{
    public string InternalName { get; init; } = null!;
    public string? Title { get; init; }
    public Asset Asset { get; init; } = null!;
}

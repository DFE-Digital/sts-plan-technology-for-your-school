using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentButtonWithEntryReferenceEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public ComponentButtonEntry Button { get; init; } = null!;
    public ContentComponent LinkToEntry { get; init; } = null!;
}

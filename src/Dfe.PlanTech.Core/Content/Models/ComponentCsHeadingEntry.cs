using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

// Type name in Contentful is CsHeading
public class ComponentCsHeadingEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public string Subtitle { get; set; } = null!;
}

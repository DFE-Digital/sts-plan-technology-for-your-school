using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentButtonWithLinkEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public ComponentButtonEntry Button { get; init; } = null!;
    public string Href { get; init; } = null!;
}

using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentButtonEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public string Value { get; init; } = null!;
    public bool IsStartButton { get; init; }
}

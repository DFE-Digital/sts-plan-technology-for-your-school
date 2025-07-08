using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentHeroEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public IEnumerable<ContentComponent> Content { get; set; } = null!;
}

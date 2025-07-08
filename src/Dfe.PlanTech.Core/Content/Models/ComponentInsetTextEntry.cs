using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentInsetTextEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
}

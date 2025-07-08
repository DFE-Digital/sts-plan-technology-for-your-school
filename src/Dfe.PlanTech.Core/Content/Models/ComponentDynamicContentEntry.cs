using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentDynamicContentEntry : Entry<ContentComponent>
{
    public string InternalName { get; init; } = null!;
    public string DynamicField { get; init; } = null!;
}

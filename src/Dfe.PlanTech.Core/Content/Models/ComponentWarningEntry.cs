
using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ComponentWarningEntry : Entry<ContentComponent>
{
    public ComponentTextBodyEntry Text { get; init; } = null!;
}

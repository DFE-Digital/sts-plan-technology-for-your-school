using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class CAndSLinkEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string LinkText { get; set; } = null!;
}

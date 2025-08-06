using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Contentful.Models;

public abstract class ContentfulEntry
{
    public SystemProperties Sys { get; set; } = null!;
    public string Description { get; set; } = null!;

    public ContentfulEntry() { }
}

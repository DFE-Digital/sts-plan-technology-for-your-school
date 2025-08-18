using Dfe.PlanTech.Core.Contentful.Interfaces;

namespace Dfe.PlanTech.Core.Contentful.Models;

public abstract class ContentfulEntry : IContentfulEntry
{
    public SystemDetails Sys { get; set; } = null!;
    public string Description { get; set; } = null!;

    public ContentfulEntry() { }
}

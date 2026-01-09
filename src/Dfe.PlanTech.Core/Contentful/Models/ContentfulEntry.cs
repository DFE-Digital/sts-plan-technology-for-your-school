using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Interfaces;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public abstract class ContentfulEntry : IContentfulEntry
{
    public string Id => Sys?.Id ?? string.Empty;
    public SystemDetails? Sys { get; set; }
    public string Description { get; set; } = null!;

    protected ContentfulEntry() { }
}

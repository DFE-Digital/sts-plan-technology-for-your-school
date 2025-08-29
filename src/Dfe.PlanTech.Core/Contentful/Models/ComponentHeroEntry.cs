using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ComponentHeroEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public IEnumerable<ContentfulEntry> Content { get; set; } = null!;
}

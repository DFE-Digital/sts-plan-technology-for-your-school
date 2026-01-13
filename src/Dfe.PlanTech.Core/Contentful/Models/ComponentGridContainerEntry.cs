using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ComponentGridContainerEntry : ContentfulEntry
{
    public string? InternalName { get; set; }
    public IReadOnlyList<ComponentCardEntry>? Content { get; set; }
}

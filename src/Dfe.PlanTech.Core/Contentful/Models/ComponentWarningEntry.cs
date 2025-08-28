using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ComponentWarningEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public ComponentTextBodyEntry Text { get; init; } = null!;
}

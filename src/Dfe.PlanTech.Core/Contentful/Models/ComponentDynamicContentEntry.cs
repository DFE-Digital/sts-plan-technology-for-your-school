using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ComponentDynamicContentEntry : ContentfulEntry
{
    public string InternalName { get; init; } = null!;
    public string DynamicField { get; init; } = null!;
}


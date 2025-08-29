using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ComponentInsetTextEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
}

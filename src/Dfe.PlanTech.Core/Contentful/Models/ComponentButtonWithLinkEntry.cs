using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ComponentButtonWithLinkEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public ComponentButtonEntry Button { get; init; } = null!;
    public string Href { get; init; } = null!;
}

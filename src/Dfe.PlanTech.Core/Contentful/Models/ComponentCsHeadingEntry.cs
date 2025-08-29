using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ComponentCsHeadingEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public string Subtitle { get; set; } = null!;
}

using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class RecommendationPageEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public PageEntry Page { get; set; } = null!;
}

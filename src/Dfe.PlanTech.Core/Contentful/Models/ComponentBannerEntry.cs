using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ComponentBannerEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public bool HasButton { get; set; }
    public string ButtonText { get; init; } = null!;
    public string Url { get; init; } = null!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

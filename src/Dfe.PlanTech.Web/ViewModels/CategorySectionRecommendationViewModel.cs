using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class CategorySectionRecommendationViewModel
{
    public string? NoRecommendationFoundErrorMessage { get; init; }
    public string? RecommendationDisplayName { get; init; }
    public string? RecommendationSlug { get; init; }
    public string? SectionName { get; init; }
    public string? SectionSlug { get; init; }
    public bool? Viewed { get; init; }
}

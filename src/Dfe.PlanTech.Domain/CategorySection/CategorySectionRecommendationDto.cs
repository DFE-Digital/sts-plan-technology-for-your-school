namespace Dfe.PlanTech.Domain.CategorySection;

public class CategorySectionRecommendationDto
{
    public string? RecommendationSlug { get; init; }

    public string? RecommendationDisplayName { get; init; }

    public string? NoRecommendationFoundErrorMessage { get; init; }

    public string? SectionSlug { get; init; }

    public bool? Viewed { get; init; }
}

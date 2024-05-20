namespace Dfe.PlanTech.Domain.CategorySection;

public class CategorySectionRecommendationDto
{
    public string? RecommendationSlug { get; init; }

    public string? RecommendationDisplayName { get; init; }

    public string? NoRecommendationFoundErrorMessage { get; init; }

    public string? SectionSlug { get; init; }

    public string TagColour => RecommendationSlug != null ? Constants.TagColour.LightBlue : Constants.TagColour.Grey ;

    public string TagText => RecommendationSlug != null ? "READY" : "NOT AVAILABLE";
    
}

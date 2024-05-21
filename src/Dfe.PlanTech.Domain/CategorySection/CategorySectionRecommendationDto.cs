using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.CategorySection;

public class CategorySectionRecommendationDto
{
    public string? RecommendationSlug { get; init; }

    public string? RecommendationDisplayName { get; init; }

    public string? NoRecommendationFoundErrorMessage { get; init; }

    public string? SectionSlug { get; init; }

    public Tag Tag => RecommendationSlug != null
        ? new Tag("READY", TagColour.LightBlue)
        : new Tag("NOT AVAILABLE", TagColour.Grey);
}

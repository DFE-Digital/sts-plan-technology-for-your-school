namespace Dfe.PlanTech.Web.Models
{
    public class RecommendationsViewComponentViewModel
    {
        public string? RecommendationSlug { get; init; }

        public string? RecommendationDisplayName { get; init; }

        public string? NoRecommendationFoundErrorMessage { get; init; }

        public string? SectionSlug { get; init; }
    }
}
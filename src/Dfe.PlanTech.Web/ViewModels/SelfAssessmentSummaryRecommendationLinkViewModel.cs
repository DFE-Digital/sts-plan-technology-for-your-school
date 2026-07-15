namespace Dfe.PlanTech.Web.ViewModels
{
    public class SelfAssessmentSummaryRecommendationLinkViewModel
    {
        public string LinkText { get; set; } = string.Empty;

        public string Href { get; set; } = string.Empty;

        public string? SchoolUrn { get; init; }

        public string? SchoolName { get; init; }

        public string? CategorySlug { get; init; }
    }
}

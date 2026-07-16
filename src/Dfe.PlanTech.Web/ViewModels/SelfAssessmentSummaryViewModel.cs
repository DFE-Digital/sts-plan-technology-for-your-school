using Dfe.PlanTech.Core.Constants;

namespace Dfe.PlanTech.Web.ViewModels
{
    public class SelfAssessmentSummaryViewModel
    {
        public string SectionName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public int? CompletedSchoolCount { get; set; }

        public bool IsMatSummary { get; set; }

        public List<SelfAssessmentSummaryRecommendationLinkViewModel> RecommendationLinks { get; set; } = [];

        public bool ShowSubmitAnotherSelfAssessment { get; set; }

        public string SubmitAnotherSelfAssessmentHref { get; set; } = string.Empty;

        public string BackToHomeHref { get; set; } = UrlConstants.HomePage;
    }
}

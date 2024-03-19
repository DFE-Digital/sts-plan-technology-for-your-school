using Dfe.PlanTech.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models
{
    public class RecommendationsViewComponentViewModel
    {
        public string? RecommendationSlug { get; init; }

        public string? RecommendationDisplayName { get; init; }

        public string? NoRecommendationFoundErrorMessage { get; init; }

        public string? SectionSlug { get; init; }

        public RecommendationsViewComponentViewModel()
        {

        }

        public RecommendationsViewComponentViewModel(string errorMessage)
        {
            NoRecommendationFoundErrorMessage = errorMessage;
        }


        public RecommendationsViewComponentViewModel(RecommendationsViewDto viewDto, string sectionSlug)
        {
            RecommendationSlug = viewDto.RecommendationSlug;
            RecommendationDisplayName = viewDto.DisplayName;
            SectionSlug = sectionSlug;
        }
    }
}
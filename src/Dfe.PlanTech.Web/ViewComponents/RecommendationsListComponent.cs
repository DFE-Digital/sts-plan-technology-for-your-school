using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents
{
    class RecommendationsList : ViewComponent
    {
        private readonly ILogger<RecommendationsList> _logger;

        public RecommendationsList(ILogger<RecommendationsList> logger)
        {
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(ICategory category)
        {
            var recommendationPageList = _GetRecommendations(category);
            return View(recommendationPageList);
        }

        private List<RecommendationPage?> _GetRecommendations(ICategory category)
        {
            List<RecommendationPage?> recommendationPageList = new List<RecommendationPage?>();

            foreach (ISection section in category.Sections)
            {
                var sectionMaturity = category.SectionStatuses.Where(sectionStatus => sectionStatus.SectionId == section.Sys.Id && sectionStatus.Completed == 1)
                    .Select(sectionStatus => sectionStatus.Maturity)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(sectionMaturity)) continue;

                var recommendation = section.GetRecommendationForMaturity(sectionMaturity);

                if (recommendation == null)
                {
                    // TODO: Log Recommendation Not Found
                }

                recommendationPageList.Add(recommendation);
            }

            return recommendationPageList;
        }
    }
}
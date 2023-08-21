using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents
{
    public class Recommendations : ViewComponent
    {
        private readonly ILogger<Recommendations> _logger;

        public Recommendations(ILogger<Recommendations> logger)
        {
            _logger = logger;
        }

        public IViewComponentResult Invoke(ICategory category)
        {
            var recommendationWithSectionNameList = category.Completed == 1 ? _GetRecommendationsWithSectionNames(category) : null;
            return View(recommendationWithSectionNameList);
        }

        private IEnumerable<(RecommendationPage? Recommendation, string SectionName)> _GetRecommendationsWithSectionNames(ICategory category)
        {
            foreach (ISection section in category.Sections)
            {
                var sectionMaturity = category.SectionStatuses.Where(sectionStatus => sectionStatus.SectionId == section.Sys.Id && sectionStatus.Completed == 1)
                    .Select(sectionStatus => sectionStatus.Maturity)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(sectionMaturity)) continue;

                var recommendation = section.GetRecommendationForMaturity(sectionMaturity);

                if (recommendation == null) _logger.LogWarning("No Recommendation Found: Section - {sectionName}, Maturity - {sectionMaturity}", section.Name, sectionMaturity);

                yield return (recommendation, section.Name);
            }
        }
    }
}
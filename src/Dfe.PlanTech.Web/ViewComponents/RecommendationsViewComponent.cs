using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents
{
    public class RecommendationsViewComponent : ViewComponent
    {
        private readonly ILogger<RecommendationsViewComponent> _logger;
        private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;

        public RecommendationsViewComponent(ILogger<RecommendationsViewComponent> logger, IGetSubmissionStatusesQuery getSubmissionStatusesQuery)
        {
            _logger = logger;
            _getSubmissionStatusesQuery = getSubmissionStatusesQuery;
        }

        public IViewComponentResult Invoke(ICategory category)
        {
            category = _getSubmissionStatusesQuery.GetCategoryWithCompletedSectionStatuses(category);

            var recommendationsViewComponentViewModel = category.Completed >= 1 ? _GetRecommendationsViewComponentViewModel(category) : null;

            return View(recommendationsViewComponentViewModel);
        }

        private IEnumerable<RecommendationsViewComponentViewModel> _GetRecommendationsViewComponentViewModel(ICategory category)
        {
            foreach (ISection section in category.Sections)
            {
                var sectionMaturity = category.SectionStatuses.Where(sectionStatus => sectionStatus.SectionId == section.Sys.Id && sectionStatus.Completed == 1)
                    .Select(sectionStatus => sectionStatus.Maturity)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(sectionMaturity)) continue;

                var recommendation = section.GetRecommendationForMaturity(sectionMaturity);

                if (recommendation == null) _logger.LogError("No Recommendation Found: Section - {sectionName}, Maturity - {sectionMaturity}", section.Name, sectionMaturity);

                yield return new RecommendationsViewComponentViewModel()
                {
                    RecommendationSlug = recommendation?.Page.Slug,
                    RecommendationDisplayName = recommendation?.DisplayName,
                    NoRecommendationFoundErrorMessage = recommendation == null ? String.Format("Unable to retrieve {0} recommendation", section.Name) : null
                };
            }
        }
    }
}
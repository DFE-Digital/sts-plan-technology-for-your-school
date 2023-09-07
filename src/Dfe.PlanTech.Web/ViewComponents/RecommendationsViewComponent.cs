using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents
{
    public class RecommendationsViewComponent : ViewComponent
    {
        private readonly ILogger<RecommendationsViewComponent> _logger;
        private readonly IGetSubmissionStatusesQuery _query;

        public RecommendationsViewComponent(ILogger<RecommendationsViewComponent> logger,
            IGetSubmissionStatusesQuery query)
        {
            _logger = logger;
            _query = query;
        }

        public async Task<IViewComponentResult> InvokeAsync(ICategory[] categories)
        {
            var allSectionsOfCombinedCategories = new List<ISection>(categories.Length * 10);
            var sectionStatuses = await _query.GetSectionSubmissionStatuses(categories.SelectMany(cat => cat.Sections));

            var recommendationsAvailable = false;

            foreach (var category in categories)
            {
                if (category.Completed >= 1)
                {
                    recommendationsAvailable = true;
                }

                category.SectionStatuses = sectionStatuses.Where(sectionStatus => category.Sections.Any(section => section.Sys.Id == sectionStatus.SectionId))
                                                            .ToList();
                category.Completed = category.SectionStatuses.Count(x => x.Completed == 1);
                category.RetrievalError = false;
                allSectionsOfCombinedCategories.AddRange(category.Sections);
            }

            var recommendationsViewComponentViewModel =
                recommendationsAvailable
                    ? _GetRecommendationsViewComponentViewModel(allSectionsOfCombinedCategories.ToArray(),
                        sectionStatuses)
                    : null;

            return View(recommendationsViewComponentViewModel);
        }

        private IEnumerable<RecommendationsViewComponentViewModel> _GetRecommendationsViewComponentViewModel(
            ISection[] sections, IList<SectionStatuses> sectionStatusesList)
        {
            foreach (ISection section in sections)
            {
                var sectionMaturity = sectionStatusesList.Where(sectionStatus =>
                        sectionStatus.SectionId == section.Sys.Id && sectionStatus.Completed == 1)
                    .Select(sectionStatus => sectionStatus.Maturity)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(sectionMaturity)) continue;

                var recommendation = section.GetRecommendationForMaturity(sectionMaturity);

                if (recommendation == null)
                    _logger.LogError("No Recommendation Found: Section - {sectionName}, Maturity - {sectionMaturity}",
                        section.Name, sectionMaturity);

                yield return new RecommendationsViewComponentViewModel()
                {
                    RecommendationSlug = recommendation?.Page.Slug,
                    RecommendationDisplayName = recommendation?.DisplayName,
                    NoRecommendationFoundErrorMessage = recommendation == null
                        ? string.Format("Unable to retrieve {0} recommendation", section.Name)
                        : null
                };
            }
        }

        // public ICategory RetrieveSectionStatuses(ICategory category)
        // {
        //     try
        //     {
        //         category.SectionStatuses = _query.GetSectionSubmissionStatuses(category.Sections).ToList();
        //         category.Completed = category.SectionStatuses.Count(x => x.Completed == 1);
        //         category.RetrievalError = false;
        //         return category;
        //     }
        //     catch (Exception e)
        //     {
        //         _logger.LogError(
        //             "An exception has occurred while trying to retrieve section progress with the following message - {}",
        //             e.Message);
        //         category.RetrievalError = true;
        //         return category;
        //     }
        // }
    }
}
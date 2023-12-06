using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

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

    public IViewComponentResult Invoke(IEnumerable<Category> categories)
    {
        var allSectionsOfCombinedCategories = new List<Section>();
        var allSectionStatusesOfCombinedCategories = new List<SectionStatusDto>();

        var recommendationsAvailable = false;
        foreach (var category in categories)
        {
            if (category.Completed >= 1)
            {
                recommendationsAvailable = true;
            }

            var categoryElement = RetrieveSectionStatuses(category);
            allSectionsOfCombinedCategories.AddRange(categoryElement.Sections);
            allSectionStatusesOfCombinedCategories.AddRange(categoryElement.SectionStatuses);
        }

        var recommendationsViewComponentViewModel =
            recommendationsAvailable
                ? GetRecommendationsViewComponentViewModel(allSectionsOfCombinedCategories,
                    allSectionStatusesOfCombinedCategories)
                : null;

        return View(recommendationsViewComponentViewModel);
    }

    private IEnumerable<RecommendationsViewComponentViewModel> GetRecommendationsViewComponentViewModel(
        IEnumerable<Section> sections, List<SectionStatusDto> sectionStatusesList)
    {
        foreach (var section in sections)
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
                SectionSlug = section.InterstitialPage.Slug,
                NoRecommendationFoundErrorMessage = recommendation == null
                    ? string.Format("Unable to retrieve {0} recommendation", section.Name)
                    : null
            };
        }
    }

    public Category RetrieveSectionStatuses(Category category)
    {
        try
        {
            category.SectionStatuses = _query.GetSectionSubmissionStatuses(category.Sections).ToList();
            category.Completed = category.SectionStatuses.Count(x => x.Completed == 1);
            category.RetrievalError = false;

            return category;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "An exception has occurred while trying to retrieve section progress with the following message - {}",
                e.Message);
            category.RetrievalError = true;
            return category;
        }
    }
}
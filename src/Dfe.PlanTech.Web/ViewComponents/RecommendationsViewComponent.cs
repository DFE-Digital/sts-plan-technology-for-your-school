using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class RecommendationsViewComponent(
    ILogger<RecommendationsViewComponent> logger,
    IGetSubmissionStatusesQuery query,
    IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery) : ViewComponent
{
    private readonly ILogger<RecommendationsViewComponent> _logger = logger;
    private readonly IGetSubmissionStatusesQuery _query = query;
    private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery = getSubTopicRecommendationQuery;

    public async Task<IViewComponentResult> InvokeAsync(IEnumerable<ICategoryComponent> categories)
    {
        var allSectionsOfCombinedCategories = new List<ISectionComponent>();
        var allSectionStatusesOfCombinedCategories = new List<SectionStatusDto>();

        var recommendationsAvailable = false;
        foreach (var category in categories)
        {
            if (category.Completed >= 1)
            {
                recommendationsAvailable = true;
            }

            var categoryElement = await RetrieveSectionStatuses(category);
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

    private async IAsyncEnumerable<RecommendationsViewComponentViewModel> GetRecommendationsViewComponentViewModel(
        IEnumerable<ISectionComponent> sections, List<SectionStatusDto> sectionStatusesList)
    {
        foreach (var section in sections)
        {
            var sectionMaturity = sectionStatusesList.Where(sectionStatus => sectionStatus.SectionId == section.Sys.Id)
                                                    .Select(sectionStatus => sectionStatus.Maturity)
                                                    .FirstOrDefault();

            if (string.IsNullOrEmpty(sectionMaturity)) continue;


            var recommendation = await _getSubTopicRecommendationQuery.GetRecommendationsViewDto(section.Sys.Id, sectionMaturity, default);

            if (recommendation == null)
            {
                _logger.LogError("No Recommendation Found: Section - {sectionName}, Maturity - {sectionMaturity}",
                    section.Name, sectionMaturity);

                yield return new RecommendationsViewComponentViewModel(string.Format("Unable to retrieve {0} recommendation", section.Name));
                continue;
            }

            if (section.InterstitialPage?.Slug == null)
            {
                _logger.LogError("No Slug found for Subtopic with ID: {SectionId}  / name: {SectionName}", section.Sys.Id, section.Name);
            }

            yield return new RecommendationsViewComponentViewModel(recommendation, section.InterstitialPage?.Slug ?? "");
        }
    }

    public async Task<ICategoryComponent> RetrieveSectionStatuses(ICategoryComponent category)
    {
        try
        {
            category.SectionStatuses = await _query.GetSectionSubmissionStatuses(category.Sections);
            category.Completed = category.SectionStatuses.Count(x => x.Completed == 1);
            category.RetrievalError = false;

            return category;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "An exception has occurred while trying to retrieve section progress with the following message - {errorMessage}",
                e.Message);
            category.RetrievalError = true;
            return category;
        }
    }
}
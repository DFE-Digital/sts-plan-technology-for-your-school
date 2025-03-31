using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Groups;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class GroupsDashboardViewComponent(
    ILogger<GroupsDashboardViewComponent> logger,
    IGetSubmissionStatusesQuery query,
    IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery,
    [FromServices] ISystemTime systemTime) : ViewComponent
{
    private readonly ILogger<GroupsDashboardViewComponent> _logger = logger;
    private readonly IGetSubmissionStatusesQuery _query = query;
    private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery = getSubTopicRecommendationQuery;

    public async Task<IViewComponentResult> InvokeAsync(Category category)
    {
        var viewModel = await GenerateViewModel(category);

        return View(viewModel);
    }

    private async Task<GroupsDashboardViewComponentViewModel> GenerateViewModel(Category category)
    {
        if (category.Sections.Count == 0)
        {
            _logger.LogError("Found no sections for category {id}", category.Sys.Id);

            return new GroupsDashboardViewComponentViewModel
            {
                NoSectionsErrorRedirectUrl = "ServiceUnavailable"
            };
        }

        category = await RetrieveSectionStatuses(category);

        return new GroupsDashboardViewComponentViewModel
        {
            GroupsCategorySectionDto = await GetGroupsCategorySectionDto(category).ToListAsync(),
        };
    }

    private async IAsyncEnumerable<GroupsCategorySectionDto> GetGroupsCategorySectionDto(Category category)
    {
        foreach (var section in category.Sections)
        {
            var sectionStatus = category.SectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId == section.Sys.Id);

            if (string.IsNullOrWhiteSpace(section.InterstitialPage?.Slug))
                _logger.LogError("No Slug found for Subtopic with ID: {sectionId}/ name: {sectionName}", section.Sys.Id, section.Name);

            yield return new GroupsCategorySectionDto(
                slug: section.InterstitialPage?.Slug,
                name: section.Name,
                retrievalError: category.RetrievalError,
                sectionStatus: sectionStatus,
                systemTime
            );
        }
    }

    public async Task<Category> RetrieveSectionStatuses(Category category)
    {
        try
        {
            category.SectionStatuses = await _query.GetSectionSubmissionStatuses(category.Sections);
            category.Completed = category.SectionStatuses.Count(x => x.Completed);
            category.RetrievalError = false;
            return category;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                             "An exception has occurred while trying to retrieve section progress with the following message - {message}",
                             e.Message);
            category.RetrievalError = true;
            return category;
        }
    }
}

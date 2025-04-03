using Dfe.PlanTech.Domain.CategorySection;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Groups;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class GroupsDashboardViewComponent(ILogger<GroupsDashboardViewComponent> logger,
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
        var schoolIdData = ViewData["SchoolId"] as string;
        int schoolId = Convert.ToInt32(schoolIdData);

        category = await RetrieveSectionStatuses(category, schoolId);

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
                recommendation: await GetCategorySectionRecommendationDto(section, sectionStatus),
                systemTime: systemTime
            );
        }
    }

    public async Task<Category> RetrieveSectionStatuses(Category category, int schoolId)
    {
        try
        {
            category.SectionStatuses = await _query.GetSectionSubmissionStatuses(category.Sections, schoolId);
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

    private async Task<CategorySectionRecommendationDto> GetCategorySectionRecommendationDto(ISectionComponent section, SectionStatusDto? sectionStatus)
    {
        if (string.IsNullOrEmpty(sectionStatus?.LastMaturity))
            return new CategorySectionRecommendationDto();

        try
        {
            var recommendation = await _getSubTopicRecommendationQuery.GetRecommendationsViewDto(section.Sys.Id, sectionStatus.LastMaturity);
            if (recommendation == null)
            {
                return new CategorySectionRecommendationDto
                {
                    NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
                };
            }
            return new CategorySectionRecommendationDto
            {
                RecommendationSlug = recommendation.RecommendationSlug,
                RecommendationDisplayName = recommendation.DisplayName,
                SectionSlug = section.InterstitialPage?.Slug,
                Viewed = sectionStatus.Viewed
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                             "An exception has occurred while trying to retrieve the recommendation for Section {sectionName}, with the message {errMessage}",
                             section.Name,
                             e.Message);
            return new CategorySectionRecommendationDto
            {
                NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
            };
        }
    }
}

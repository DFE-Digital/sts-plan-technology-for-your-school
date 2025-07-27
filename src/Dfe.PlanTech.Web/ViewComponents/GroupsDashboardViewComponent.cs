using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class GroupsDashboardViewComponent(
    ILogger<GroupsDashboardViewComponent> logger,
    CurrentUser currentUser
) : ViewComponent
{
    private readonly ILogger<GroupsDashboardViewComponent> _logger = logger;
    private readonly CurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

    public async Task<IViewComponentResult> InvokeAsync(CmsCategoryDto category)
    {
        var viewModel = await GenerateViewModel(category);

        return View(viewModel);
    }

    private async Task<GroupsDashboardViewComponentViewModel> GenerateViewModel(CmsCategoryDto category)
    {
        if (category.Sections.Count == 0)
        {
            _logger.LogError("Found no sections for category {id}", category.Sys.Id);

            return new GroupsDashboardViewComponentViewModel
            {
                NoSectionsErrorRedirectUrl = "ServiceUnavailable"
            };
        }

        var userId = await _user.GetCurrentUserId();
        var userEstablishmentId = await _user.GetEstablishmentId();
        var selectedSchool = await _getGroupSelectionQuery.GetLatestSelectedGroupSchool(userId.Value, userEstablishmentId)
            ?? throw new DatabaseException($"Could not get latest selected group school for user with ID {userId.Value} in establishment: {userEstablishmentId}");

        category = await SubmissionStatusHelpers.RetrieveSectionStatuses(category, _logger, _query, selectedSchool.SelectedEstablishmentId);

        return new GroupsDashboardViewComponentViewModel
        {
            Description = category.Content is { Count: > 0 } content ? content[0] : new MissingComponent(),
            GroupsCategorySectionDto = await GetGroupsCategorySectionDto(category).ToListAsync(),
            ProgressRetrievalErrorMessage = category.RetrievalError
                ? "Unable to retrieve progress, please refresh your browser."
                : null
        };
    }

    private async IAsyncEnumerable<GroupsCategorySectionDto> GetGroupsCategorySectionDto(Category category)
    {
        foreach (var section in category.Sections)
        {
            var sectionStatus = category.SectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId == section.Sys.Id);

            if (string.IsNullOrWhiteSpace(section.InterstitialPage?.Slug))
                _logger.LogError("No Slug found for Subtopic with ID: {sectionId}/ name: {sectionName}", section.Sys.Id, section.Name);

            var recommendation = await GetCategorySectionRecommendationDto(section, sectionStatus);

            var recommendationSlug = !string.IsNullOrEmpty(recommendation.SectionSlug) ? $"{UrlConstants.GroupsSlug}/recommendations/{recommendation.SectionSlug}" : "";

            yield return new GroupsCategorySectionDto(
                slug: section.InterstitialPage?.Slug,
                name: section.Name,
                recommendationSlug: recommendationSlug,
                retrievalError: category.RetrievalError,
                sectionStatus: sectionStatus,
                recommendation: recommendation,
                systemTime: systemTime);
        }
    }

    private async Task<CategorySectionRecommendationDto> GetCategorySectionRecommendationDto(ISectionComponent section, SectionStatusDto? sectionStatus)
    {
        if (string.IsNullOrEmpty(sectionStatus?.LastMaturity))
            return new CategorySectionRecommendationDto();

        try
        {
            var recommendation = await _getSubtopicRecommendationQuery.GetRecommendationsViewDto(section.Sys.Id, sectionStatus.LastMaturity);
            if (recommendation == null)
            {
                return new CategorySectionRecommendationDto
                {
                    NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
                };
            }
            return new CategorySectionRecommendationDto
            {
                RecommendationSlug = "recommendations",
                RecommendationDisplayName = recommendation.DisplayName,
                SectionSlug = section.InterstitialPage?.Slug,
                SectionName = section.Name,
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

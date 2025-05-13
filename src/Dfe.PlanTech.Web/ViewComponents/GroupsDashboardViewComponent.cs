using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.CategorySection;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Groups;
using Dfe.PlanTech.Domain.Groups.Interfaces;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class GroupsDashboardViewComponent(ILogger<GroupsDashboardViewComponent> logger,
        IGetSubmissionStatusesQuery query, IGetGroupSelectionQuery getGroupSelectionQuery, IUser user,
        [FromServices] ISystemTime systemTime, IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery) : ViewComponent
{
    private readonly ILogger<GroupsDashboardViewComponent> _logger = logger;
    private readonly IGetSubmissionStatusesQuery _query = query;
    private readonly IGetGroupSelectionQuery _getGroupSelectionQuery = getGroupSelectionQuery;
    private readonly IUser _user = user;
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

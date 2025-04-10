using Dfe.PlanTech.Domain.Groups;
using Dfe.PlanTech.Domain.Groups.Interfaces;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class GroupsDashboardViewComponent(ILogger<GroupsDashboardViewComponent> logger,
        IGetSubmissionStatusesQuery query, IGetGroupSelectionQuery getGroupSelectionQuery, IUser user,
        [FromServices] ISystemTime systemTime) : ViewComponent
{
    private readonly ILogger<GroupsDashboardViewComponent> _logger = logger;
    private readonly IGetSubmissionStatusesQuery _query = query;
    private readonly IGetGroupSelectionQuery _getGroupSelectionQuery = getGroupSelectionQuery;
    private readonly IUser _user = user;

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
        var selectedSchool = await _getGroupSelectionQuery.GetLatestSelectedGroupSchool(userId.Value, userEstablishmentId);

        category = await RetrieveSectionStatuses(category, selectedSchool.SelectedEstablishmentId);

        return new GroupsDashboardViewComponentViewModel
        {
            Description = category.Content[0],
            GroupsCategorySectionDto = GetGroupsCategorySectionDto(category).ToList(),
        };
    }

    private IEnumerable<GroupsCategorySectionDto> GetGroupsCategorySectionDto(Category category)
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
                systemTime: systemTime);
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
}

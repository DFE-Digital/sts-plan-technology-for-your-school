using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class GroupsViewBuilder(
    CurrentUser currentUser,
    ContentfulService contentfulService,
    EstablishmentService establishmentService
)
{
    private const string selectASchoolViewName = "~/Views/Groups/GroupsSelectSchool.cshtml";

    private readonly CurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly ContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    private readonly EstablishmentService _establishmentService = establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));

    public async Task<IActionResult> GetSelectASchoolView(Controller controller)
    {
        var establishmentId = _currentUser.EstablishmentId
           ?? throw new InvalidDataException(nameof(currentUser.EstablishmentId));

        var selectASchoolPageContent = await _contentfulService.GetPageBySlug(UrlConstants.GroupsSelectionPageSlug);
        var dashboardContent = await _contentfulService.GetPageBySlug(UrlConstants.GroupsDashboardSlug);

        var categories = dashboardContent.Content.OfType<CmsCategoryDto>();
        var schools = await _establishmentService.GetGroupEstablishments(establishmentId);
        var groupSchools = await _establishmentService.BuildSchoolsWithSubmissionCountsView(categories, schools);

        var groupName = _currentUser.GetEstablishmentModel().OrgName;
        var title = groupName;
        List<CmsEntryDto> content = selectASchoolPageContent?.Content ?? [];

        string totalSections = string.Empty;
        if (categories != null)
        {
            totalSections = categories.Sum(category => category.Sections.Count).ToString();
        }

        var contactLink = await getNavigationQuery.GetLinkById(contactOptions.Value.LinkId, cancellationToken);

        var viewModel = new GroupsSelectorViewModel
        {
            GroupName = groupName,
            GroupEstablishments = groupSchools,
            Title = title,
            Content = content,
            TotalSections = totalSections,
            ProgressRetrievalErrorMessage = String.IsNullOrEmpty(totalSections)
                ? "Unable to retrieve progress"
                : null,
            ContactLinkHref = contactLink?.Href
        };

        controller.ViewData["Title"] = "Select a school";

        return controller.View(selectASchoolViewName, viewModel);
    }
}

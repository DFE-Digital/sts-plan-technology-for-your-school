using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class GroupsViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IContentfulService contentfulService,
    IEstablishmentService establishmentService,
    ISubmissionService submissionService,
    ICurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser), IGroupsViewBuilder
{
    private readonly IEstablishmentService _establishmentService = establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly ISubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly ContactOptionsConfiguration _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));

    private const string SelectASchoolViewName = "GroupsSelectSchool";


    public async Task<IActionResult> RouteToSelectASchoolViewModelAsync(Controller controller)
    {
        var establishmentId = GetEstablishmentIdOrThrowException();

        var selectASchoolPageContent = await ContentfulService.GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug);

        // Categories which would display on the home page - use these to figure out which categories we should get counts for
        // This means updating Contentful will impact what is considered in the totals
        var homeSlug = UrlConstants.HomePage.Replace("/", "");
        var homePage = await ContentfulService.GetPageBySlugAsync(homeSlug);
        var categories = homePage.Content?.OfType<QuestionnaireCategoryEntry>().ToList();

        if (categories is null || !categories.Any())
        {
            throw new InvalidDataException("There are no categories to display for the selected page.");
        }

        var groupSchools = await _establishmentService.GetEstablishmentLinksWithSubmissionStatusesAndCounts(categories, establishmentId);

        var groupName = CurrentUser.Organisation?.Name;
        var title = groupName ?? "Your organisation";
        List<ContentfulEntry> content = selectASchoolPageContent?.Content ?? [];

        string totalSections = categories.Sum(category => category.Sections.Count).ToString();

        var contactLink = await ContentfulService.GetLinkByIdAsync(_contactOptions.LinkId);

        var viewModel = new GroupsSelectorViewModel
        {
            GroupName = groupName,
            GroupEstablishments = groupSchools,
            Title = new ComponentTitleEntry(title),
            Content = content,
            TotalSections = totalSections,
            ProgressRetrievalErrorMessage = String.IsNullOrEmpty(totalSections)
                ? "Unable to retrieve progress"
                : null,
            ContactLinkHref = contactLink?.Href
        };

        controller.ViewData["Title"] = "Select a school";
        return controller.View(SelectASchoolViewName, viewModel);
    }

    public async Task RecordGroupSelectionAsync(string selectedEstablishmentUrn, string selectedEstablishmentName)
    {
        var userDsiReference = GetDsiReferenceOrThrowException();

        await _establishmentService.RecordGroupSelection(
            userDsiReference,
            CurrentUser.EstablishmentId,
            CurrentUser.Organisation,
            selectedEstablishmentUrn,
            selectedEstablishmentName
        );
    }

}

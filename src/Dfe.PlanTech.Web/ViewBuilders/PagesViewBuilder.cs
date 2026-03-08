using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class PagesViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IOptions<ErrorPagesConfiguration> errorPages,
    IContentfulService contentfulService,
    ICurrentUser currentUser,
    IEstablishmentService establishmentService,
    INotifyService notifyService,
    ISubmissionService submissionService,
    IRecommendationService recommendationService
) : BaseViewBuilder(logger, contentfulService, currentUser), IPagesViewBuilder
{
    public const string CategoryLandingPageView =
        "~/Views/Recommendations/CategoryLandingPage.cshtml";
    public const string CategoryLandingPagePrintView =
        "~/Views/Recommendations/CategoryLandingPrintContent.cshtml";

    private readonly ContactOptionsConfiguration _contactOptions =
        contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
    private readonly ErrorPagesConfiguration _errorPages =
        errorPages.Value ?? throw new ArgumentNullException(nameof(errorPages));
    private readonly INotifyService _notifyService =
        notifyService ?? throw new ArgumentNullException(nameof(notifyService));
    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly IRecommendationService _recommendationService =
        recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));

    public async Task<IActionResult> RouteBasedOnOrganisationTypeAsync(
        Controller controller,
        PageEntry page
    )
    {
        var needsToSelectSchool =
            page.RequiresAuthorisation
            && CurrentUser.UserOrganisationIsGroup
            && CurrentUser.GroupSelectedSchoolUrn is null;
        if (needsToSelectSchool)
        {
            return controller.Redirect(UrlConstants.SelectASchoolPage);
        }

        // If the selected URN isn't valid (doesn't exist, isn't within the current user's trust, etc.), redirect them to the select a school page.
        var hasSelectedASchool =
            CurrentUser.UserOrganisationIsGroup && CurrentUser.GroupSelectedSchoolUrn is not null;
        if (hasSelectedASchool)
        {
            // Named `establishmentId`, but for a group (e.g. MAT) this is the internal PlanTech synthetic database ID for the group not the selected establishment.
            var groupId =
                CurrentUser.UserOrganisationId
                ?? throw new InvalidDataException(
                    "User is a MAT user but does not have an organisation ID (for the group)"
                );
            var groupSchools =
                await establishmentService.GetEstablishmentLinksWithRecommendationCounts(groupId);

            var selectedSchoolIsValid = groupSchools.Any(s =>
                s.Urn.Equals(CurrentUser.GroupSelectedSchoolUrn)
            );
            if (!selectedSchoolIsValid)
            {
                return controller.Redirect(UrlConstants.SelectASchoolPage);
            }
        }

        if (page.IsLandingPage == true)
        {
            var category = await ContentfulService.GetCategoryBySlugAsync(page.Slug, 4);
            if (category is null)
            {
                throw new ContentfulDataUnavailableException(
                    $"Could not find category at {controller.Request.Path.Value}"
                );
            }
            var landingPageViewModel = await BuildLandingPageViewModelAsync(
                controller,
                category,
                page.Slug
            );
            return controller.View(CategoryLandingPageView, landingPageViewModel);
        }

        controller.ViewData[ViewDataConstants.Title] =
            StringExtensions.UseNonBreakingHyphenAndHtmlDecode(page.Title?.Text)
            ?? PageTitleConstants.PlanTechnologyForYourSchool;

        var viewModel = new PageViewModel(page)
        {
            MicrocopyEntries = await ContentfulService.GetMicrocopyEntriesAsync(),
        };

        if (page.DisplayOrganisationName)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                Logger.LogWarning(
                    "Tried to display establishment on {PageTitle} but user is not authenticated",
                    page.Title?.Text ?? page.Id
                );
            }
            else
            {
                viewModel.ActiveEstablishmentName =
                    await CurrentUser.GetActiveEstablishmentNameAsync();
                viewModel.ActiveEstablishmentUrn =
                    await CurrentUser.GetActiveEstablishmentUrnAsync();
            }
        }

        if (string.Equals(page.Id, _errorPages.InternalErrorPageId))
        {
            viewModel.DisplayBlueBanner = false;
        }

        return controller.View("Page", viewModel);
    }

    public async Task<IActionResult> RouteToCategoryLandingPrintPageAsync(
        Controller controller,
        string categorySlug
    )
    {
        var category = await ContentfulService.GetCategoryBySlugAsync(categorySlug, 4);
        if (category is null)
        {
            return controller.RedirectToHomePage();
        }

        var landingPageViewModel = await BuildLandingPageViewModelAsync(
            controller,
            category,
            categorySlug
        );

        return controller.View(CategoryLandingPagePrintView, landingPageViewModel);
    }

    public async Task<IActionResult> RouteToShareStandardPageAsync(
        Controller controller,
        string categorySlug,
        ShareByEmailInputViewModel? inputModel = null
    )
    {
        var category = await ContentfulService.GetCategoryBySlugAsync(categorySlug, 4);
        if (category is null)
        {
            return controller.RedirectToHomePage();
        }

        var viewModel = BuildShareByEmailViewModel(
            nameof(PagesController),
            nameof(PagesController.ShareStandard),
            category,
            null,
            categorySlug,
            null,
            null,
            inputModel
        );

        if (inputModel is null || !controller.ModelState.IsValid)
        {
            return controller.View(ShareByEmailViewName, viewModel);
        }

        var establishmentName =
            await CurrentUser.GetActiveEstablishmentNameAsync()
            ?? throw new InvalidDataException(
                "Cannot send an email without an active establishment name"
            );

        var establishmentId = await GetActiveEstablishmentIdOrThrowException();
        var recommendationStatuses =
            await _recommendationService.GetLatestRecommendationStatusesAsync(establishmentId);

        var sectionIds = category.Sections.Select(s => s.Id).ToList();

        var sectionStatuses = await _submissionService.GetSectionStatusesForSchoolAsync(
            establishmentId,
            sectionIds
        );

        var results = _notifyService.SendStandardEmail(
            inputModel.ToModel(),
            category.Sections,
            sectionStatuses,
            recommendationStatuses,
            category.Header.Text,
            establishmentName
        );

        controller.ViewData[ViewDataConstants.NotifySendResults] = results;

        var page = await ContentfulService.GetPageBySlugAsync(categorySlug);
        return await RouteBasedOnOrganisationTypeAsync(controller, page);
    }

    private async Task<CategoryLandingPageViewModel> BuildLandingPageViewModelAsync(
        Controller controller,
        QuestionnaireCategoryEntry category,
        string categorySlug
    )
    {
        var microcopy = await ContentfulService.GetMicrocopyEntriesAsync();

        return new CategoryLandingPageViewModel
        {
            Slug = categorySlug,
            BeforeTitleContent = category.LandingPage?.BeforeTitleContent ?? [],
            Title = new ComponentTitleEntry(category.Header.Text),
            Category = category,
            SectionName = controller.TempData["SectionName"] as string,
            SortOrder = controller.Request.Query["sort"],
            MicrocopyEntries = microcopy,
        };
    }

    public async Task<NotFoundViewModel> BuildNotFoundViewModel()
    {
        var contactLink = await ContentfulService.GetLinkByIdAsync(_contactOptions.LinkId);
        return new NotFoundViewModel { ContactLinkHref = contactLink?.Href };
    }
}

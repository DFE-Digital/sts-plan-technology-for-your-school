using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class PagesViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IOptions<ErrorPagesConfiguration> errorPages,
    IContentfulService contentfulService,
    IEstablishmentService establishmentService,
    ICurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser), IPagesViewBuilder
{
    public const string CategoryLandingPageView = "~/Views/Recommendations/CategoryLandingPage.cshtml";
    public const string CategoryLandingPagePrintView = "~/Views/Recommendations/CategoryLandingPrintContent.cshtml";

    private readonly ContactOptionsConfiguration _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
    private readonly ErrorPagesConfiguration _errorPages = errorPages?.Value ?? throw new ArgumentNullException(nameof(errorPages));

    public async Task<IActionResult> RouteBasedOnOrganisationTypeAsync(Controller controller, PageEntry page)
    {
        var needsToSelectSchool = CurrentUser.UserOrganisationIsGroup && CurrentUser.GroupSelectedSchoolUrn is null;
        if (needsToSelectSchool)
        {
            return controller.Redirect(UrlConstants.SelectASchoolPage);
        }

        // If the selected URN isn't valid (doesn't exist, isn't within the current user's trust, etc.), redirect them to the select a school page.
        var hasSelectedASchool = CurrentUser.UserOrganisationIsGroup && CurrentUser.GroupSelectedSchoolUrn is not null;
        if (hasSelectedASchool)
        {
            // Named `establishmentId`, but for a group (e.g. MAT) this is the internal PlanTech synthetic database ID for the group not the selected establishment.
            var groupId = CurrentUser.UserOrganisationId ?? throw new InvalidDataException("User is a MAT user but does not have an organisation ID (for the group)");
            var groupSchools = await establishmentService.GetEstablishmentLinksWithSubmissionStatusesAndCounts(
                [],
                groupId
            );

            var selectedSchoolIsValid = groupSchools.Any(s => s.Urn.Equals(CurrentUser.GroupSelectedSchoolUrn));
            if (!selectedSchoolIsValid)
            {
                return controller.Redirect(UrlConstants.SelectASchoolPage);
            }
        }

        if (page.IsLandingPage == true)
        {
            var landingPageViewModel = await BuildLandingPageAsync(controller, page.Slug);
            return controller.View(CategoryLandingPageView, landingPageViewModel);
        }

        controller.ViewData["Title"] = StringExtensions.UseNonBreakingHyphenAndHtmlDecode(page.Title?.Text)
            ?? PageTitleConstants.PlanTechnologyForYourSchool;

        var viewModel = new PageViewModel(page);

        if (page.DisplayOrganisationName)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                Logger.LogWarning("Tried to display establishment on {page} but user is not authenticated", page.Title?.Text ?? page.Id);
            }
            else
            {
                viewModel.ActiveEstablishmentName = await CurrentUser.GetActiveEstablishmentNameAsync();
                viewModel.ActiveEstablishmentUrn = await CurrentUser.GetActiveEstablishmentUrnAsync();
            }
        }

        if (string.Equals(page.Id, _errorPages.InternalErrorPageId))
        {
            viewModel.DisplayBlueBanner = false;
        }

        return controller.View("Page", viewModel);
    }

    public async Task<IActionResult> RouteToCategoryLandingPrintPageAsync(Controller controller, string categorySlug)
    {
        var category = await ContentfulService.GetCategoryBySlugAsync(categorySlug);
        if (category is null)
        {
            return controller.RedirectToHomePage();
        }

        var viewModel = await BuildLandingPageAsync(controller, categorySlug);

        return controller.View(CategoryLandingPagePrintView, viewModel);
    }

    private async Task<CategoryLandingPageViewModel> BuildLandingPageAsync(Controller controller, string categorySlug)
    {
        var category = await ContentfulService.GetCategoryBySlugAsync(categorySlug, 4);
        if (category is null)
        {
            throw new ContentfulDataUnavailableException($"Could not find category at {controller.Request.Path.Value}");
        }

        return new CategoryLandingPageViewModel
        {
            Slug = categorySlug,
            Title = new ComponentTitleEntry(category.Header.Text),
            Category = category,
            SectionName = controller.TempData["SectionName"] as string,
            SortOrder = controller.Request.Query["sort"]
        };
    }

    public async Task<NotFoundViewModel> BuildNotFoundViewModel()
    {
        var contactLink = await ContentfulService.GetLinkByIdAsync(_contactOptions.LinkId);
        return new NotFoundViewModel { ContactLinkHref = contactLink?.Href };
    }
}

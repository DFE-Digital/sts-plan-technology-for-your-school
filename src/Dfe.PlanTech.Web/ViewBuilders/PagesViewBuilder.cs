using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Web.Context.Interfaces;
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
    ICurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser), IPagesViewBuilder
{
    public const string CategoryLandingPageView = "~/Views/Recommendations/CategoryLandingPage.cshtml";

    private readonly ContactOptionsConfiguration _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
    private readonly ErrorPagesConfiguration _errorPages = errorPages?.Value ?? throw new ArgumentNullException(nameof(errorPages));

    public async Task<IActionResult> RouteBasedOnOrganisationTypeAsync(Controller controller, PageEntry page)
    {
        if (string.Equals(page.Slug, UrlConstants.HomePage.Replace("/", "")) && CurrentUser.IsMat)
        {
            return controller.Redirect(UrlConstants.SelectASchoolPage);
        }

        if (page.IsLandingPage == true)
        {
            return await BuildLandingPageAsync(controller, page);
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
                viewModel.OrganisationName = CurrentUser.Organisation?.Name;
            }
        }

        if (string.Equals(page.Id, _errorPages.InternalErrorPageId))
        {
            viewModel.DisplayBlueBanner = false;
        }

        return controller.View("Page", viewModel);
    }

    private async Task<IActionResult> BuildLandingPageAsync(Controller controller, PageEntry page)
    {
        var category = await ContentfulService.GetCategoryBySlugAsync(page.Slug, 4);
        if (category is null)
        {
            throw new ContentfulDataUnavailableException($"Could not find category at {controller.Request.Path.Value}");
        }

        var landingPageViewModel = new CategoryLandingPageViewModel()
        {
            Slug = page.Slug,
            BeforeTitleContent = page.BeforeTitleContent,
            Title = new ComponentTitleEntry(category.Header.Text),
            Category = category,
            SectionName = controller.TempData["SectionName"] as string
        };

        return controller.View(CategoryLandingPageView, landingPageViewModel);
    }

    public async Task<NotFoundViewModel> BuildNotFoundViewModel()
    {
        var contactLink = await ContentfulService.GetLinkByIdAsync(_contactOptions.LinkId);
        return new NotFoundViewModel { ContactLinkHref = contactLink?.Href };
    }
}

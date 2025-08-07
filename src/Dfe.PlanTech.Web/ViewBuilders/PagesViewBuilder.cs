using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class PagesViewBuilder(
    ILoggerFactory loggerFactory,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IOptions<ErrorPagesConfiguration> errorPages,
    ContentfulService contentfulService,
    CurrentUser currentUser
) : BaseViewBuilder(loggerFactory, contentfulService, currentUser)
{
    public const string CategoryLandingPageView = "~/Views/Recommendations/CategoryLandingPage.cshtml";

    private ILogger<PagesViewBuilder> _logger = loggerFactory.CreateLogger<PagesViewBuilder>();
    private ContactOptionsConfiguration _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
    private ErrorPagesConfiguration _errorPages = errorPages?.Value ?? throw new ArgumentNullException(nameof(errorPages));

    public IActionResult RouteBasedOnOrganisationType(Controller controller, CmsPageDto page)
    {
        if (string.Equals(page.Slug, UrlConstants.HomePage.Replace("/", "")) && CurrentUser.IsMat)
        {
            return controller.Redirect(UrlConstants.SelectASchoolPage);
        }

        if (page.IsLandingPage == true)
        {
            return BuildLandingPage(controller, page);
        }

        controller.ViewData["Title"] = StringExtensions.UseNonBreakingHyphenAndHtmlDecode(page.Title?.Text)
            ?? PageTitleConstants.PlanTechnologyForYourSchool;

        var viewModel = new PageViewModel(page);

        if (page.DisplayOrganisationName)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                _logger.LogWarning("Tried to display establishment on {page} but user is not authenticated", page.Title?.Text ?? page.Id);
            }
            else
            {
                viewModel.OrganisationName = CurrentUser?.Organisation?.Name;
            }
        }

        if (string.Equals(page.Id, _errorPages.InternalErrorPageId))
        {
            viewModel.DisplayBlueBanner = false;
        }

        return controller.View("Page", viewModel);
    }

    private IActionResult BuildLandingPage(Controller controller, CmsPageDto page)
    {
        var category = page.Content[0] as CmsCategoryDto;
        if (category is null)
        {
            throw new ContentfulDataUnavailableException($"Could not find category at {controller.Request.Path.Value}");
        }

        var landingPageViewModel = new CategoryLandingPageViewModel()
        {
            Slug = page.Slug,
            Title = new CmsComponentTitleDto { Text = category.Header.Text },
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

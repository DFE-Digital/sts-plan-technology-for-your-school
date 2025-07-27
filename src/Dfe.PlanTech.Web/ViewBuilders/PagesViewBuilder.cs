using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Configurations;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Web.Configurations;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class PagesViewBuilder(
    ILogger<PagesViewBuilder> logger,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IOptions<ErrorPagesConfiguration> errorPages,
    CurrentUser currentUser,
    ContentfulService contentfulService
) : BaseViewBuilder(currentUser)
{
    private ILogger<PagesViewBuilder> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private ContactOptionsConfiguration _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
    private ErrorPagesConfiguration _errorPages = errorPages?.Value ?? throw new ArgumentNullException(nameof(errorPages));
    private ContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));

    public IActionResult RouteBasedOnOrganisationType(Controller controller, CmsPageDto page)
    {
        if (string.Equals(page.Slug, UrlConstants.HomePage.Replace("/", "")) && CurrentUser.IsMat)
        {
            return controller.Redirect(UrlConstants.SelectASchoolPage);
        }

        controller.ViewData["Title"] = StringExtensions.UseNonBreakingHyphenAndHtmlDecode(page.Title?.Text)
            ?? "Plan Technology For Your School";

        var viewModel = new PageViewModel(page);

        if (page.DisplayOrganisationName)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                logger.LogWarning("Tried to display establishment on {page} but user is not authenticated", page.Title?.Text ?? page.Id);
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

    public async Task<NotFoundViewModel> BuildNotFoundViewModel()
    {
        var contactLink = await _contentfulService.GetLinkByIdAsync(_contactOptions.LinkId);
        return new NotFoundViewModel { ContactLinkHref = contactLink?.Href };
    }
}

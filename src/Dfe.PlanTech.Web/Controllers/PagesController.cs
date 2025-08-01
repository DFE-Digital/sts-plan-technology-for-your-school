using System.Diagnostics;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Authorisation;
using Dfe.PlanTech.Web.Binders;
using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Route("/")]
public class PagesController(
    ILogger<PagesController> logger,
    IGetNavigationQuery getNavigationQuery,
    IOptions<ContactOptions> contactOptions,
    IOptions<ErrorPages> errorPages) : BaseController<PagesController>(logger)
{
    private readonly ContactOptions _contactOptions = contactOptions.Value;
    public const string ControllerName = "Pages";
    public const string GetPageByRouteAction = nameof(GetByRoute);
    public const string CategoryLandingPageView = "~/Views/Recommendations/CategoryLandingPage.cshtml";

    [Authorize(Policy = PageModelAuthorisationPolicy.PolicyName)]
    [HttpGet("{route?}", Name = "GetPage")]
    public IActionResult GetByRoute([ModelBinder(typeof(PageModelBinder))] Page? page, [FromServices] IUser user)
    {
        if (page == null)
        {
            logger.LogInformation("Could not find page at {Path}", Request.Path.Value);
            throw new ContentfulDataUnavailableException($"Could not find page at {Request.Path.Value}");
        }

        if (page.IsLandingPage == true)
        {
            var category = page.Content[0] as Category;

            if (category == null)
            {
                throw new ContentfulDataUnavailableException($"Could not find Category at {Request.Path.Value}");
            }

            var landingPageViewModel = new CategoryLandingPageViewModel()
            {
                Slug = page.Slug,
                Title = new Title { Text = category.Header.Text },
                Category = category,
                SectionName = TempData["SectionName"] as string
            };

            return View(CategoryLandingPageView, landingPageViewModel);
        }

        var organisationClaim = User.FindFirst("organisation")?.Value;

        if (!string.IsNullOrEmpty(organisationClaim))
        {
            var organisation = System.Text.Json.JsonSerializer.Deserialize<OrganisationDto>(organisationClaim);

            //MAT user org ID is 010
            if (page.Slug == UrlConstants.HomePage.Replace("/", "") && organisation?.Category?.Id == "010")
            {
                return Redirect(UrlConstants.SelectASchoolPage);
            }
        }

        var viewModel = new PageViewModel(page, this, user, Logger);
        if (page.Sys.Id == errorPages.Value.InternalErrorPageId)
        {
            viewModel.DisplayBlueBanner = false;
        }

        return View("Page", viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet(UrlConstants.Error, Name = UrlConstants.Error)]
    public IActionResult Error()
    => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [HttpGet(UrlConstants.NotFound, Name = UrlConstants.NotFound)]
    public async Task<IActionResult> NotFoundError()
    {
        var contactLink = await GetContactLinkAsync();

        var viewModel = new NotFoundViewModel
        {
            ContactLinkHref = contactLink?.Href
        };

        return View(viewModel);
    }

    private async Task<INavigationLink?> GetContactLinkAsync()
    {
        return await getNavigationQuery.GetLinkById(_contactOptions.LinkId);
    }
}

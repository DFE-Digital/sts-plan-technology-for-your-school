using System.Diagnostics;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
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

    [Authorize(Policy = PageModelAuthorisationPolicy.PolicyName)]
    [HttpGet("{route?}", Name = "GetPage")]
    public async Task<IActionResult> GetByRoute([ModelBinder(typeof(PageModelBinder))] Page? page, [FromServices] IUser user) 
    {
        if (page == null)
        {
            logger.LogInformation("Could not find page at {Path}", Request.Path.Value);
            throw new ContentfulDataUnavailableException($"Could not find page at {Request.Path.Value}");
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

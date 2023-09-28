using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Authorisation;
using Dfe.PlanTech.Web.Binders;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Dfe.PlanTech.Web.Controllers;

public class PagesController : BaseController<PagesController>
{
    public PagesController(ILogger<PagesController> logger) : base(logger)
    {
    }

    [Authorize(Policy = PageModelAuthorisationPolicy.POLICY_NAME)]
    [HttpGet("/{route?}")]
    [HttpGet("~/{sectionSlug}/recommendation/{route?}", Name = "GetPageByRouteAndSection")]
    public IActionResult GetByRoute([ModelBinder(typeof(PageModelBinder))] Page page, [FromServices] IUser user)
    {
        var viewModel = CreatePageModel(page, user);

        return View("Page", viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route(UrlConstants.Error)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route(UrlConstants.ServiceUnavailable)]
    public IActionResult ServiceUnavailable()
    {
        return View(new ServiceUnavailableViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private PageViewModel CreatePageModel(Page page, IUser user)
    {
        ViewData["Title"] = System.Net.WebUtility.HtmlDecode(page.Title?.Text) ?? "Plan Technology For Your School";

        var viewModel = new PageViewModel()
        {
            Page = page,
        };

        viewModel.TryLoadOrganisationName(HttpContext, user, logger);

        return viewModel;
    }
}
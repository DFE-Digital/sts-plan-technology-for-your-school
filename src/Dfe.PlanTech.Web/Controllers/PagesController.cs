using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Authorisation;
using Dfe.PlanTech.Web.Binders;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Users.Interfaces;

namespace Dfe.PlanTech.Web.Controllers;

public class PagesController : BaseController<PagesController>
{
    public const string Controller = "Pages";
    public const string GetPageByRouteAction = nameof(GetByRoute);

    public PagesController(ILogger<PagesController> logger) : base(logger)
    {
    }

    [Authorize(Policy = PageModelAuthorisationPolicy.POLICY_NAME)]
    [HttpGet("/{route?}")]
    public IActionResult GetByRoute([ModelBinder(typeof(PageModelBinder))] Page page, [FromServices] IUser user)
    {
        var viewModel = new PageViewModel(page, this, user, logger);

        return View("Page", viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route(UrlConstants.Error)]
    public IActionResult Error()
    => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [Route(UrlConstants.ServiceUnavailable)]
    public IActionResult ServiceUnavailable()
    => View(new ServiceUnavailableViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    public IActionResult RedirectToGetByRoute(string route, Controller controller)
    => controller.RedirectToAction(nameof(GetByRoute), Controller, new { route });
}
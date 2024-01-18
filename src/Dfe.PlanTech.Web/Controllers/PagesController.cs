using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Authorisation;
using Dfe.PlanTech.Web.Binders;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Dfe.PlanTech.Web.Controllers;

public class PagesController : BaseController<PagesController>
{
    public const string ControllerName = "Pages";
    public const string GetPageByRouteAction = nameof(GetByRoute);

    public PagesController(ILogger<PagesController> logger) : base(logger)
    {
    }

    [Authorize(Policy = PageModelAuthorisationPolicy.POLICY_NAME)]
    [HttpGet("/{route?}")]
    public async Task<IActionResult> GetByRoute(string route, [FromServices] IGetPageQuery getPageQuery, [FromServices] IUser user, CancellationToken cancellationToken)
    {
        var page = await getPageQuery.GetPageBySlug(route, cancellationToken);

        if (page == null) return new NotFoundResult();

        var viewModel = new PageViewModel(page, this, user, logger);

        return View("Page", viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet(UrlConstants.Error, Name = UrlConstants.Error)]
    public IActionResult Error()
    => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [HttpGet(UrlConstants.ServiceUnavailable, Name = UrlConstants.ServiceUnavailable)]
    public IActionResult ServiceUnavailable()
    => View(new ServiceUnavailableViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
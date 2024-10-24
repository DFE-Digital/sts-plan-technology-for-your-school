using System.Diagnostics;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Authorisation;
using Dfe.PlanTech.Web.Binders;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Route("/")]
public class PagesController : BaseController<PagesController>
{
    public const string ControllerName = "Pages";
    public const string GetPageByRouteAction = nameof(GetByRoute);

    public PagesController(ILogger<PagesController> logger) : base(logger)
    {
    }

    [Authorize(Policy = PageModelAuthorisationPolicy.PolicyName)]
    [HttpGet("{route?}", Name = "GetPage")]
    public IActionResult GetByRoute([ModelBinder(typeof(PageModelBinder))] Page page, [FromServices] IUser user)
    {
        var viewModel = new PageViewModel(page, this, user, Logger);

        return View("Page", viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet(UrlConstants.Error, Name = UrlConstants.Error)]
    public IActionResult Error()
    => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [HttpGet(UrlConstants.ServiceUnavailable, Name = UrlConstants.ServiceUnavailable)]
    public IActionResult ServiceUnavailable([FromServices] IConfiguration configuration)
     => View(new ServiceUnavailableViewModel
     {
         RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
         ContactUsEmail = configuration["ContactUs:Email"]
     });

    [HttpGet(UrlConstants.NotFound, Name = UrlConstants.NotFound)]
    public IActionResult NotFoundError()
    => View();
}

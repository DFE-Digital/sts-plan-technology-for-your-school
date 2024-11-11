using System.Diagnostics;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
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
    private readonly ILogger _logger;
    private readonly IGetEntityFromContentfulQuery _getEntityFromContentfulQuery;
    public const string ControllerName = "Pages";
    public const string GetPageByRouteAction = nameof(GetByRoute);

    public PagesController(ILogger<PagesController> logger, IGetEntityFromContentfulQuery getEntityByIdQuery) : base(logger)
    {
        _logger = logger;
        _getEntityFromContentfulQuery = getEntityByIdQuery;
    }

    [Authorize(Policy = PageModelAuthorisationPolicy.PolicyName)]
    [HttpGet("{route?}", Name = "GetPage")]
    public IActionResult GetByRoute([ModelBinder(typeof(PageModelBinder))] Page? page, [FromServices] IUser user)
    {
        if (page == null)
        {
            _logger.LogInformation("Could not find page at {Path}", Request.Path.Value);
            return RedirectToAction("NotFoundError");
        }
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
    public async Task<IActionResult> NotFoundError()
    {
        string contactId = "7ezhOgrTAdhP4NeGiNj8VY";
        var contactLink = await _getEntityFromContentfulQuery.GetEntityById<NavigationLink>(contactId) ??
                _logger.LogError("Could not find navigation link with Id {ContactId}", contactId);

        var viewModel = new NotFoundViewModel
        {
            ContactLinkHref = contactLink?.Href
        };

        return View(viewModel);
    }
}

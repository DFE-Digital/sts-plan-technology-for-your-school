using System.Diagnostics;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Authorisation.Policies;
using Dfe.PlanTech.Web.Binders;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Route("/")]
public class PagesController(
    ILoggerFactory loggerFactory,
    PagesViewBuilder pagesViewBuilder
) : BaseController<PagesController>(loggerFactory)
{
    private readonly PagesViewBuilder _pagesViewBuilder = pagesViewBuilder ?? throw new ArgumentNullException(nameof(pagesViewBuilder));

    public const string ControllerName = "Pages";
    public const string GetPageByRouteAction = nameof(GetByRoute);

    [Authorize(Policy = PageModelAuthorisationPolicy.PolicyName)]
    [HttpGet("{route?}", Name = "GetPage")]
    public Task<IActionResult> GetByRoute([ModelBinder(typeof(PageModelBinder))] CmsPageDto? page)
    {
        if (page is null)
        {
            Logger.LogInformation("Could not find page at {Path}", Request.Path.Value);
            throw new ContentfulDataUnavailableException($"Could not find page at {Request.Path.Value}");
        }

        return _pagesViewBuilder.RouteBasedOnOrganisationTypeAsync(this, page);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet(UrlConstants.Error, Name = UrlConstants.Error)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [HttpGet(UrlConstants.NotFound, Name = UrlConstants.NotFound)]
    public async Task<IActionResult> NotFoundError()
    {
        var viewModel = await _pagesViewBuilder.BuildNotFoundViewModel();
        return View(viewModel);
    }
}

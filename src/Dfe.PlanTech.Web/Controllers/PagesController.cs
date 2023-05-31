using System.Diagnostics;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Core.Enums;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class PagesController : Controller
{
    private readonly ILogger<PagesController> _logger;

    public PagesController(ILogger<PagesController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index([FromServices] GetPageQuery getPageQuery)
    {
        //ar page = await getPageQuery.GetPageBySlug(nameof(Pages.Landing));

        return View("LandingPage", new Domain.Content.Models.Page() );
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
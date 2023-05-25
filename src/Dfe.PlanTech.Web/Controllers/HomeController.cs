using System.Diagnostics;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Page(string slug, [FromServices] GetPageQuery getPageQuery)
    {
        if (string.IsNullOrEmpty(slug)) throw new Exception($"{nameof(slug)} cannot be null or empty");

        var page = await getPageQuery.GetPageBySlug(slug);

        return View("Page", page);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
using System.Diagnostics;
using Dfe.ContentSupport.Web.Extensions;
using Dfe.ContentSupport.Web.Services;
using Dfe.ContentSupport.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.ContentSupport.Web.Controllers;

[Route("/content")]
[AllowAnonymous]
public class ContentController(
    [FromKeyedServices(WebApplicationBuilderExtensions.ContentAndSupportServiceKey)]
    IContentService contentService,
    [FromKeyedServices(WebApplicationBuilderExtensions.ContentAndSupportServiceKey)]
    ILayoutService layoutService,
    ILogger<ContentController> logger)
    : Controller
{
    public const string ErrorActionName = "error";
    
    [HttpGet("{slug}/{page?}")]
    public async Task<IActionResult> Index(string slug, string page = "", bool isPreview = false,
        [FromQuery] List<string>? tags = null)
    {
        if (!ModelState.IsValid)
        {
            logger.LogError(
                "Invalid model state received for {Controller} {Action} with slug {Slug}",
                nameof(ContentController), nameof(Index), slug);
            return RedirectToAction(ErrorActionName);
        }

        if (string.IsNullOrEmpty(slug))
        {
            logger.LogError("No slug received for C&S {Controller} {Action}",
                nameof(ContentController), nameof(Index));
            return RedirectToAction(ErrorActionName);
        }

        try
        {
            var resp = await contentService.GetContent(slug, isPreview);
            if (resp is null)
            {
                logger.LogError("Failed to load content for C&S page {Slug}; no content received.",
                    slug);
                return RedirectToAction(ErrorActionName);
            }

            resp = layoutService.GenerateLayout(resp, Request, page);
            ViewBag.tags = tags;

            return View("CsIndex", resp);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading C&S content page {Slug}", slug);
            return RedirectToAction(ErrorActionName);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
            { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
using Dfe.PlanTech.Web.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("[controller]")]
[AllowAnonymous]
public class ContentController(
    [FromKeyedServices(ProgramExtensions.ContentAndSupportServiceKey)]
    IContentService contentService,
    [FromKeyedServices(ProgramExtensions.ContentAndSupportServiceKey)]
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
            return RedirectToAction(ErrorActionName, PagesController.ControllerName);
        }

        if (string.IsNullOrEmpty(slug))
        {
            logger.LogError("No slug received for C&S {Controller} {Action}",
                nameof(ContentController), nameof(Index));
            return RedirectToAction(PagesController.NotFoundPage, PagesController.ControllerName);
        }

        try
        {
            var resp = await contentService.GetContent(slug, isPreview);
            if (resp is null)
            {
                logger.LogError("Failed to load content for C&S page {Slug}; no content received.",
                    slug);
                return RedirectToAction(PagesController.NotFoundPage, PagesController.ControllerName);
            }

            resp = layoutService.GenerateLayout(resp, Request, page);
            ViewBag.tags = tags!;

            return View("CsIndex", resp);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading C&S content page {Slug}", slug);
            return RedirectToAction(ErrorActionName, PagesController.ControllerName);
        }
    }
}

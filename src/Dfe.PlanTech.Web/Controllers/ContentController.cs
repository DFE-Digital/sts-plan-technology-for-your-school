﻿using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Web.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("[controller]")]
[AllowAnonymous]
public class ContentController(
    IContentService contentService,
    ILayoutService layoutService,
    ILogger<ContentController> logger) : Controller
{

    public const string ControllerName = "Content";
    public const string ErrorActionName = "error";

    [HttpGet("{slug}/{page?}")]
    public async Task<IActionResult> Index(string slug, string page = "", [FromQuery] List<string>? tags = null, CancellationToken cancellationToken = default)
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
            throw new KeyNotFoundException($"No slug received for C&S {nameof(ContentController)} {nameof(Index)}");
        }

        try
        {
            var resp = await contentService.GetContent(slug, cancellationToken);
            if (resp is null)
            {
                logger.LogError("Failed to load content for C&S page {Slug}; no content received.",
                    slug);
                throw new ContentfulDataUnavailableException($"Failed to load content for C&S page {slug}; no content received.");
            }

            resp = layoutService.GenerateLayout(resp, Request, page);
            ViewBag.tags = tags!;

            return View("CsIndex", resp);
        }
        catch (Exception ex) when (ex is not ContentfulDataUnavailableException)
        {
            logger.LogError(ex, "Error loading C&S content page {Slug}", slug);
            return RedirectToAction(ErrorActionName, PagesController.ControllerName);
        }
    }
}

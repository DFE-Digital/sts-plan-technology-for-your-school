﻿using Dfe.ContentSupport.Web.Extensions;
using Dfe.ContentSupport.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.ContentSupport.Web.Controllers;

public class SitemapController(
    [FromKeyedServices(WebApplicationBuilderExtensions.ContentAndSupportServiceKey)]
    IContentService contentfulService) : Controller
{
    [HttpGet]
    [Route("/sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/";
        var sitemap = await contentfulService.GenerateSitemap(baseUrl);
        return Content(sitemap, "application/xml");
    }
}

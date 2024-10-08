using System.Xml.Linq;
using Dfe.ContentSupport.Web.Extensions;
using Dfe.ContentSupport.Web.Models.Mapped;
using Dfe.ContentSupport.Web.ViewModels;

namespace Dfe.ContentSupport.Web.Services;

public class ContentService(
    [FromKeyedServices(WebApplicationBuilderExtensions.ContentAndSupportServiceKey)]
    IContentfulService contentfulService,
    [FromKeyedServices(WebApplicationBuilderExtensions.ContentAndSupportServiceKey)]
    ICacheService<List<CsPage>> cache,
    [FromKeyedServices(WebApplicationBuilderExtensions.ContentAndSupportServiceKey)]
    IModelMapper modelMapper)
    : IContentService
{
    public async Task<CsPage?> GetContent(string slug, bool isPreview = false)
    {
        var resp = await GetContentSupportPages(nameof(ContentSupportPage.Slug), slug, isPreview);
        return resp is not null && resp.Count != 0 ? resp[0] : null;
    }

    public async Task<string> GenerateSitemap(string baseUrl)
    {
        var resp =
            await GetContentSupportPages(nameof(ContentSupportPage.IsSitemap), "true", false);

        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var sitemap = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(xmlns + "urlset", new XAttribute("xmlns", xmlns),
                from url in resp
                select
                    new XElement(xmlns + "url",
                        new XElement(xmlns + "loc", $"{baseUrl}{url.Slug}"),
                        new XElement(xmlns + "changefreq", "yearly")
                    )
            )
        );

        return sitemap.ToString();
    }

    public async Task<List<CsPage>> GetCsPages(bool isPreview = true)
    {
        var pages =
            await GetContentSupportPages(nameof(ContentSupportPage.IsSitemap), "true", isPreview);
        return pages.ToList();
    }

    public async Task<List<CsPage>> GetContentSupportPages(
        string field, string value, bool isPreview)
    {
        var key = $"{field}_{value}";
        if (!isPreview)
        {
            var fromCache = cache.GetFromCache(key);
            if (fromCache is not null) return fromCache;
        }


        var result = await contentfulService.GetContentSupportPages(field, value);
        var pages = modelMapper.MapToCsPages(result);

        if (!isPreview) cache.AddToCache(key, pages);

        return pages;
    }
}
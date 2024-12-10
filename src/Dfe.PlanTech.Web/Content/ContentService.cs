using System.Xml.Linq;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Domain.Content.Queries;

namespace Dfe.PlanTech.Web.Content;
public class ContentService : IContentService // This indicates that ContentService implements IContentService
{
    private readonly IGetContentSupportPageQuery _getContentSupportPageQuery;
    private readonly IModelMapper _modelMapper;

    public ContentService(
        [FromKeyedServices(ProgramExtensions.ContentAndSupportServiceKey)]
        ICacheService<List<CsPage>> cache,
        [FromKeyedServices(ProgramExtensions.ContentAndSupportServiceKey)]
        IModelMapper modelMapper,
        IGetContentSupportPageQuery getContentSupportPageQuery)
    {
        _getContentSupportPageQuery = getContentSupportPageQuery;
        _modelMapper = modelMapper;
    }

    public async Task<CsPage?> GetContent(string slug, bool isPreview = false)
    {
        var resp = await GetContentSupportPages(nameof(ContentSupportPage.Slug), slug, isPreview);
        return resp is not null && resp.Count != 0 ? resp.FirstOrDefault(page => page.Slug == slug) : null;
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

    public async Task<List<CsPage>> GetContentSupportPages(string field, string value, bool isPreview)
    {
        var result = await _getContentSupportPageQuery.GetContentSupportPages();
        var pages = _modelMapper.MapToCsPages(result);

        return pages;
    }
}

using Dfe.ContentSupport.Web.Models.Mapped;

namespace Dfe.ContentSupport.Web.Services;

public interface IContentService
{
    Task<CsPage?> GetContent(string slug, bool isPreview = false);
    Task<string> GenerateSitemap(string baseUrl);
    Task<List<CsPage>> GetCsPages(bool isPreview = true);
}
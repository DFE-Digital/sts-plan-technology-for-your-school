using Dfe.PlanTech.Web.Models.Content.Mapped;

namespace Dfe.PlanTech.Web.Content;

public interface IContentService
{
    Task<CsPage?> GetContent(string slug, bool isPreview = false);
    Task<string> GenerateSitemap(string baseUrl);
    Task<List<CsPage>> GetCsPages(bool isPreview = true);
}

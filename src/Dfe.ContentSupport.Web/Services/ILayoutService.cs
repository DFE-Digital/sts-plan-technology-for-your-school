using Dfe.ContentSupport.Web.Models.Mapped;

namespace Dfe.ContentSupport.Web.Services;

public interface ILayoutService
{
    CsPage GenerateLayout(CsPage page, HttpRequest request, string pageSlug);
}
using Dfe.PlanTech.Web.Models.Content.Mapped;

namespace Dfe.PlanTech.Web.Content;

public interface ILayoutService
{
    CsPage GenerateLayout(CsPage page, HttpRequest request, string pageSlug);
}

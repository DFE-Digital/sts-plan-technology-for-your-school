using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface IRecommendationsViewBuilder
    {
        Task<IActionResult> RouteToSingleRecommendation(Controller controller, string categorySlug, string sectionSlug, string chunkSlug, bool isPrintView);
        Task<IActionResult> RouteBySectionAndRecommendation(Controller controller, string categorySlug, string sectionSlug, bool isPrintView);
    }
}

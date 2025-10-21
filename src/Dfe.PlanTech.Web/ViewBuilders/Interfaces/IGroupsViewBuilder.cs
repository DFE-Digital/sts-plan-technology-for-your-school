using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface IGroupsViewBuilder
    {
        Task<IActionResult> RouteToSelectASchoolViewModelAsync(Controller controller);
        Task RecordGroupSelectionAsync(string selectedEstablishmentUrn, string selectedEstablishmentName);
        Task<IActionResult> RouteToSchoolDashboardViewAsync(Controller controller);
        Task<IActionResult> RouteToGroupsRecommendationAsync(Controller controller, string sectionSlug);
        Task<IActionResult> RouteToRecommendationsPrintViewAsync(Controller controller, string sectionSlug, string schoolName);
    }
}

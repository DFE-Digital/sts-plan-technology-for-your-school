using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface IGroupsViewBuilder
    {
        Task<IActionResult> RouteToSelectASchoolViewModelAsync(Controller controller);
        Task RecordGroupSelectionAsync(string selectedEstablishmentUrn, string selectedEstablishmentName);
    }
}

using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface IGroupsViewBuilder
    {
        Task<IActionResult> RouteToSelectASchoolViewModelAsync(Controller controller);
        Task<IActionResult> RouteToSelectASelfAssessmentViewModelAsync(Controller controller);
        Task RecordGroupSelectionAsync(
            string selectedEstablishmentUrn,
            string selectedEstablishmentName
        );
        Task<IActionResult> RouteToSelectSchoolsToAssessViewModelAsync(
            Controller controller,
            string sectionSlug,
            GroupSelectSchoolsToAssessInputViewModel? inputViewModel = null
        );

        Task<IActionResult> SubmitSelectedSchoolsToAssessAndRedirect(
            Controller controller,
            string sectionSlug,
            GroupSelectSchoolsToAssessInputViewModel inputViewModel
        );
    }
}

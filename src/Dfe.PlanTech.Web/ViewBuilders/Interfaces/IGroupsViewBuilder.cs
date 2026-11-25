using Dfe.PlanTech.Web.ViewModels;
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
            GroupsSelectSchoolsToAssessViewModel? viewModel = null
        );

        Task<IActionResult> SubmitSelectedSchoolsToAssessAndRedirect(
            Controller controller,
            string sectionSlug,
            GroupsSelectSchoolsToAssessViewModel viewModel
         );

        Task<IActionResult> RouteToViewInProgressAnswers(
            Controller controller,
            string categorySlug,
            string sectionSlug,
            string schoolUrn
        );

        Task<IActionResult> RouteToSelectSchoolsToUpdateStatusViewModelAsync(
            Controller controller,
            string sectionSlug,
            string recommendationSlug,
            GroupsSelectSchoolsToUpdateStatusViewModel? viewModel = null
        );

        Task<IActionResult> RouteToSelectStatusToUpdateViewModelAsync(
            Controller controller,
            string sectionSlug,
            string recommendationSlug,
            GroupsSelectStatusToUpdateViewModel? viewModel = null
        );
        Task<IActionResult> RouteToMatStandardsListAsync(Controller controller);

        Task<IActionResult> UpdateSchoolsRecommendationStatusAsync(
            Controller controller,
            string sectionSlug,
            string recommendationSlug,
            GroupRecommendationInputViewModel viewModel
        );
    }
}

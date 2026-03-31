using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface IPagesViewBuilder
    {
        Task<IActionResult> RouteBasedOnOrganisationTypeAsync(
            Controller controller,
            PageEntry page
        );
        Task<IActionResult> RouteToCategoryLandingPrintPageAsync(
            Controller controller,
            string categorySlug
        );
        Task<IActionResult> RouteToShareStandardPageAsync(
            Controller controller,
            string categorySlug,
            ShareByEmailInputViewModel? inputModel = null
        );
        Task<NotFoundViewModel> BuildNotFoundViewModelAsync();

        NotifyShareResultsViewModel? BuildNotifyShareResultsViewModel(Controller controller);
    }
}

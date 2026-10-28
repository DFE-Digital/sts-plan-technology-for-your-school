using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
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
            string categorySlug,
            CategoryLandingContext context = CategoryLandingContext.School
        );
        Task<IActionResult> RouteToShareStandardPageAsync(
            Controller controller,
            string categorySlug,
            ShareByEmailInputViewModel? inputModel = null,
            CategoryLandingContext context = CategoryLandingContext.School
        );
        Task<IActionResult> RouteToMatCategoryLandingPageAsync(
            Controller controller,
            string categorySlug
        );
        Task<NotFoundViewModel> BuildNotFoundViewModelAsync();

        NotifyShareResultsViewModel? BuildNotifyShareResultsViewModel(Controller controller);
    }
}

using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces;

public interface IRecommendationsViewBuilder
{
    Task<IActionResult> RouteToSingleRecommendation(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string chunkSlug,
        bool useChecklist,
        CategoryLandingContext context = CategoryLandingContext.School
    );
    Task<IActionResult> RouteBySectionAndRecommendation(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        bool useChecklist,
        string? singleChunkSlug,
        string? originatingSlug
    );

    Task<IActionResult> UpdateRecommendationStatusAsync(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string chunkSlug,
        SingleRecommendationInputViewModel inputModel
    );

    Task<IActionResult> RouteToPrintSingle(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string chunkSlug
    );

    Task<IActionResult> RouteToPrintAll(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string chunkSlug
    );

    Task<IActionResult> RouteToShareRecommendationAsync(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string chunkSlug,
        ShareByEmailInputViewModel? inputModel = null
    );
}

using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces;

public interface IRecommendationsViewBuilder
{
    Task<IActionResult> RouteToSingleRecommendation(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string chunkSlug,
        bool useChecklist
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
        string selectedStatus,
        string? notes
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
}

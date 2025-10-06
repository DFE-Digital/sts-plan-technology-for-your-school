using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Authorize]
[Route("/")]
public class RecommendationsController(
    ILogger<RecommendationsController> logger,
    IRecommendationsViewBuilder recommendationsViewBuilder
)
    : BaseController<RecommendationsController>(logger)
{
    private readonly IRecommendationsViewBuilder _recommendationsViewBuilder = recommendationsViewBuilder ?? throw new ArgumentNullException(nameof(recommendationsViewBuilder));

    public const string ControllerName = "Recommendations";
    public const string GetSingleRecommendationAction = nameof(GetSingleRecommendation);

    [HttpGet("{categorySlug}/{sectionSlug}/recommendations/{chunkSlug}", Name = GetSingleRecommendationAction)]
    public async Task<IActionResult> GetSingleRecommendation(string categorySlug, string sectionSlug, string chunkSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug, nameof(categorySlug));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(chunkSlug, nameof(chunkSlug));

        return await _recommendationsViewBuilder.RouteToSingleRecommendation(this, categorySlug, sectionSlug, chunkSlug, false);
    }

    [HttpGet("{categorySlug}/{sectionSlug}/recommendations/print", Name = "GetRecommendationChecklist")]
    public async Task<IActionResult> GetRecommendationChecklist(string categorySlug, string sectionSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug, nameof(categorySlug));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));

        return await _recommendationsViewBuilder.RouteBySectionAndRecommendation(this, categorySlug, sectionSlug, true);
    }
}

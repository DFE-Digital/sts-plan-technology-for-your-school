using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Authorize]
[Route("/")]
public class RecommendationsController(
    ILogger<RecommendationsController> logger,
    RecommendationsViewBuilder recommendationsViewBuilder
)
    : BaseController<RecommendationsController>(logger)
{
    private readonly RecommendationsViewBuilder _recommendationsViewBuilder = recommendationsViewBuilder ?? throw new ArgumentNullException(nameof(recommendationsViewBuilder));

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

    [HttpGet("{sectionSlug}/recommendation/preview/{maturity?}", Name = "GetRecommendationPreview")]
    public async Task<IActionResult> GetRecommendationPreview(string sectionSlug, string? maturity)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));

        return await _recommendationsViewBuilder.RouteBySectionSlugAndMaturity(this, sectionSlug, maturity);
    }

    [HttpGet("{categorySlug}/{sectionSlug}/recommendations/print", Name = "GetRecommendationChecklist")]
    public async Task<IActionResult> GetRecommendationChecklist(string categorySlug, string sectionSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug, nameof(categorySlug));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));

        return await _recommendationsViewBuilder.RouteBySectionAndRecommendation(this, categorySlug, sectionSlug, true);
    }
}

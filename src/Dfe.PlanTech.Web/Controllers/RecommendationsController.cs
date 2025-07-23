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
    private readonly ILogger<RecommendationsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly RecommendationsViewBuilder _recommendationsViewBuilder = recommendationsViewBuilder ?? throw new ArgumentNullException(nameof(recommendationsViewBuilder));

    public const string ControllerName = "Recommendations";
    public const string GetRecommendationAction = "GetRecommendation";

    [HttpGet("{sectionSlug}/recommendation/{recommendationSlug}", Name = GetRecommendationAction)]
    public async Task<IActionResult> GetRecommendation(string sectionSlug, string recommendationSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(recommendationSlug, nameof(recommendationSlug));

        return await _recommendationsViewBuilder.RouteBySectionAndRecommendation(this, sectionSlug, recommendationSlug, false);
    }

    [HttpGet("{sectionSlug}/recommendation/preview/{maturity?}", Name = "GetRecommendationPreview")]
    public async Task<IActionResult> GetRecommendationPreview(string sectionSlug, string? maturity)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);

        return await _recommendationsViewBuilder.RouteBySectionSlugAndMaturity(this, sectionSlug, maturity);
    }

    [HttpGet("{sectionSlug}/recommendation/{recommendationSlug}/print", Name = "GetRecommendationChecklist")]
    public async Task<IActionResult> GetRecommendationChecklist(string sectionSlug, string recommendationSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(recommendationSlug, nameof(recommendationSlug));

        return await _recommendationsViewBuilder.RouteBySectionAndRecommendation(this, sectionSlug, recommendationSlug, false);
    }

    [HttpGet("from-section/{sectionSlug}")]
    public async Task<IActionResult> FromSection(string sectionSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));

        return await _recommendationsViewBuilder.RouteFromSection(this, sectionSlug);
    }
}


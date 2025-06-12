using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Authorize]
[Route("/")]
public class RecommendationsController(ILogger<RecommendationsController> logger)
    : BaseController<RecommendationsController>(logger)
{
    public const string ControllerName = "Recommendations";
    public const string GetRecommendationAction = "GetRecommendation";

    [HttpGet("{sectionSlug}/recommendation/{recommendationSlug}", Name = GetRecommendationAction)]
    public async Task<IActionResult> GetRecommendation(string sectionSlug,
                                                       string recommendationSlug,
                                                       [FromServices] IGetRecommendationRouter getRecommendationValidator,
                                                       CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(recommendationSlug))
            throw new ArgumentNullException(nameof(recommendationSlug));

        return await getRecommendationValidator.ValidateRoute(sectionSlug,
          recommendationSlug,
          false,
          this,
          cancellationToken);
    }

    [HttpGet("{sectionSlug}/recommendation/preview/{maturity?}", Name = "GetRecommendationPreview")]
    public async Task<IActionResult> GetRecommendationPreview(string sectionSlug,
                                                              string? maturity,
                                                              [FromServices] ContentfulOptions contentfulOptions,
                                                              [FromServices] IGetRecommendationRouter getRecommendationRouter,
                                                              CancellationToken cancellationToken)
    {
        if (!contentfulOptions.UsePreviewApi)
        {
            return new RedirectResult(UrlConstants.SelfAssessmentPage);
        }

        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        return await getRecommendationRouter.GetRecommendationPreview(sectionSlug, maturity, this, cancellationToken);
    }

    [HttpGet("{sectionSlug}/recommendation/{recommendationSlug}/print", Name = "GetRecommendationChecklist")]
    public async Task<IActionResult> GetRecommendationChecklist(string sectionSlug,
                                                                string recommendationSlug,
                                                                [FromServices] IGetRecommendationRouter getRecommendationValidator,
                                                                CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(recommendationSlug))
            throw new ArgumentNullException(nameof(recommendationSlug));

        return await getRecommendationValidator.ValidateRoute(sectionSlug,
            recommendationSlug,
            true,
            this,
            cancellationToken);
    }

    [HttpGet("from-section/{sectionSlug}")]
    public async Task<IActionResult> FromSection(
        string sectionSlug,
        [FromServices] IGetRecommendationRouter getRecommendationRouter,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        var recommendationSlug = await getRecommendationRouter.GetRecommendationSlugForSection(sectionSlug, cancellationToken);
        return RedirectToAction(nameof(GetRecommendation), new { recommendationSlug });
    }
}


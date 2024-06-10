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
    public const string GetRecommendationAction = nameof(GetRecommendation);

    [HttpGet("{sectionSlug}/recommendation/{recommendationSlug}", Name = "GetRecommendation")]
    public async Task<IActionResult> GetRecommendation(string sectionSlug,
                                                       string recommendationSlug,
                                                       [FromServices] IGetRecommendationRouter getRecommendationValidator,
                                                       CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(recommendationSlug)) throw new ArgumentNullException(nameof(recommendationSlug));

        return await getRecommendationValidator.ValidateRoute(sectionSlug,
          recommendationSlug,
          this,
          cancellationToken);
    }
}
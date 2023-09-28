using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dfe.PlanTech.Web.Routing;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class RecommendationsController : BaseController<RecommendationsController>
{
  public const string GetRecommendationAction = nameof(GetRecommendation);

  public RecommendationsController(ILogger<RecommendationsController> logger) : base(logger)
  {
  }

  [HttpGet("{sectionSlug}/recommendation/{recommendationSlug}", Name = "GetRecommendation")]
  public Task<IActionResult> GetRecommendation(string sectionSlug,
                                                     string recommendationSlug,
                                                     [FromServices] IGetRecommendationRouter getRecommendationValidator,
                                                     CancellationToken cancellationToken)
    => getRecommendationValidator.ValidateRoute(sectionSlug,
        recommendationSlug,
        this,
        cancellationToken);
}
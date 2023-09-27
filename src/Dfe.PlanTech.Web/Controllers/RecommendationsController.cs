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
  public async Task<IActionResult> GetRecommendation(string sectionSlug,
                                                     string recommendationSlug,
                                                     [FromServices] UserJourneyRouter userJourneyRouter,
                                                     [FromServices] GetRecommendationValidator getRecommendationValidator,
                                                     CancellationToken cancellationToken)
  {
    await userJourneyRouter.GetJourneyStatusForSection(sectionSlug, cancellationToken);

    return await getRecommendationValidator.ValidateRoute(sectionSlug,
                                                          recommendationSlug,
                                                          userJourneyRouter,
                                                          this,
                                                          cancellationToken);
  }
}
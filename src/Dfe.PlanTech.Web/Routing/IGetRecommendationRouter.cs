using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public interface IGetRecommendationRouter
{
  public Task<IActionResult> ValidateRoute(string sectionSlug,
                                           string recommendationSlug,
                                           RecommendationsController controller,
                                           CancellationToken cancellationToken);
}
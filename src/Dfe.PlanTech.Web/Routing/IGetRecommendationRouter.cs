using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

/// <summary>
/// Router for "Recommendation" page under <see cref="RecommendationsController"/> 
/// </summary>
public interface IGetRecommendationRouter
{
    /// <summary>
    /// Gets current user journey status, then either returns Recommendation page for slug (if appropriate), 
    /// or redirects to correct next part of user journey
    /// </summary>
    /// <param name="sectionSlug"></param>
    /// <param name="recommendationSlug"></param>
    /// <param name="checklist"></param>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IActionResult> ValidateRoute(string sectionSlug,
                                             string recommendationSlug,
                                             bool checklist,
                                             RecommendationsController controller,
                                             CancellationToken cancellationToken);
}

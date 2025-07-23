using Dfe.PlanTech.Domain.Questionnaire.Models;
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


    /// <summary>
    /// Gets a preview of a recommendation. Includes all recommendation chunks; no filtering off user's journey
    /// </summary>
    /// <param name="sectionSlug"></param>
    /// <param name="maturity">What intro to return for the user. If null, returns first found.</param>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IActionResult> GetRecommendationPreview(string sectionSlug,
                                                        string? maturity,
                                                        RecommendationsController controller,
                                                        CancellationToken cancellationToken);

    /// <summary>
    /// Get the slug of the recommendation for a completed section by slug
    /// </summary>
    /// <param name="sectionSlug"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<string> GetRecommendationSlugForSection(string sectionSlug, CancellationToken cancellationToken);


    /// <summary>
    /// Get a single recommendation chunk for a completed section by section slug and slugified chunk header
    /// </summary>
    /// <param name="sectionSlug"></param>
    /// <param name="recommendationSlug"></param>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<(Section, RecommendationChunk, List<RecommendationChunk>)> GetSingleRecommendation(string sectionSlug,
                                                                                          string recommendationSlug,
                                                                                          RecommendationsController controller,
                                                                                          CancellationToken cancellationToken);
}

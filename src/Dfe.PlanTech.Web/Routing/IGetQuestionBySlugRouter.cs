using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

/// <summary>
/// Router for "Question" page under <see cref="QuestionsController"/> 
/// </summary>
public interface IGetQuestionBySlugRouter
{
    /// <summary>
    /// Gets current user journey status, then either returns Question page for question slug (if appropriate), 
    /// or redirects to correct next part of user journey
    /// </summary>
    /// <param name="sectionSlug"></param>
    /// <param name="questionSlug"></param>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IActionResult> ValidateRoute(string sectionSlug,
                                             string questionSlug,
                                             QuestionsController controller,
                                             CancellationToken cancellationToken);
}

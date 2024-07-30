using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

/// <summary>
/// Router for "Check Answers" page under <see cref="CheckAnswersController"/> 
/// </summary>
public interface ICheckAnswersRouter
{
    /// <summary>
    /// Gets current user journey status, then either returns Check Answers page (if appropriate), 
    /// or redirects to correct next part of user journey
    /// </summary>
    /// <param name="sectionSlug"></param>
    /// <param name="errorMessage"></param>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IActionResult> ValidateRoute(string sectionSlug,
                                      string? errorMessage,
                                      CheckAnswersController controller,
                                      CancellationToken cancellationToken);
}

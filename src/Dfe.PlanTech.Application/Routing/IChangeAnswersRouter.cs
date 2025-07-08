using Dfe.PlanTech.Application.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Application.Routing;

/// <summary>
/// Router for "Change Answers" page under <see cref="ChangeAnswersController"/> 
/// </summary>
public interface IChangeAnswersRouter
{
    /// <summary>
    /// Gets current user journey status, then either returns Change Answers page (if appropriate), 
    /// or redirects to correct next part of user journey
    /// </summary>
    /// <param name="sectionSlug"></param>
    /// <param name="errorMessage"></param>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IActionResult> ValidateRoute(string sectionSlug,
                                      string? errorMessage,
                                      ChangeAnswersController controller,
                                      CancellationToken cancellationToken);
}

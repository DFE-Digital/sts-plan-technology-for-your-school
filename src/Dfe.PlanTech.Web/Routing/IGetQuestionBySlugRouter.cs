using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public interface IGetQuestionBySlugRouter
{
 public Task<IActionResult> ValidateRoute(string sectionSlug,
                                          string questionSlug,
                                          QuestionsController controller,
                                          CancellationToken cancellationToken);
}
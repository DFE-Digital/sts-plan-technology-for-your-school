using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public interface ICheckAnswersRouter
{
  Task<IActionResult> ValidateRoute(string sectionSlug,
                                    CheckAnswersController controller,
                                    CancellationToken cancellationToken);
}
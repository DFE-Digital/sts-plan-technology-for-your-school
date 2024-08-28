using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Establishments.Exceptions;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Dfe.PlanTech.Domain.Users.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Dfe.PlanTech.Domain.Exceptions;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Dfe.PlanTech.Web.Controllers;

namespace Dfe.PlanTech.Web.Middleware;

public class UserJourneyMissingContentExceptionHandler
{
  private readonly ILogger<UserJourneyMissingContentExceptionHandler> _logger;
  private readonly IDeleteCurrentSubmissionCommand _deleteCurrentSubmissionCommand;
  private readonly IConfiguration _configuration;

  public UserJourneyMissingContentExceptionHandler(ILogger<UserJourneyMissingContentExceptionHandler> logger,
                                                  IDeleteCurrentSubmissionCommand deleteCurrentSubmissionCommand,
                                                  IConfiguration configuration)
  {
    _logger = logger;
    _configuration = configuration;
    _deleteCurrentSubmissionCommand = deleteCurrentSubmissionCommand;
  }

  public async Task<IActionResult> Handle(
      Controller controller,
      UserJourneyMissingContentException exception,
      CancellationToken cancellationToken)
  {
    _logger.LogError(exception, "Handling errored user journey for section {Section}", exception.Section);

    await _deleteCurrentSubmissionCommand.DeleteCurrentSubmission(exception.Section, cancellationToken);

    controller.TempData["SubtopicError"] = _configuration["ErrorMessages:ConcurrentUsersOrContentChange"];

    return controller.RedirectToAction(PagesController.GetPageByRouteAction, PagesController.ControllerName, new { route = "self-assessment" });
  }
}

using Dfe.PlanTech.Domain.Exceptions;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Middleware;

public class UserJourneyMissingContentExceptionHandler : IUserJourneyMissingContentExceptionHandler
{
    public const string ErrorMessageConfigKey = "ErrorMessages:ConcurrentUsersOrContentChange";
    public const string ErrorMessageTempDataKey = "SubtopicError";

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

    public async Task<IActionResult> Handle(Controller controller, UserJourneyMissingContentException exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Handling errored user journey for section {Section}", exception.Section.Name);

        await _deleteCurrentSubmissionCommand.DeleteCurrentSubmission(exception.Section, cancellationToken);

        controller.TempData[ErrorMessageTempDataKey] = _configuration[ErrorMessageConfigKey];

        return controller.RedirectToAction(PagesController.GetPageByRouteAction, PagesController.ControllerName, new { route = "self-assessment" });
    }
}

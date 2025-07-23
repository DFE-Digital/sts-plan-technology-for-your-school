using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Handlers;

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

    public async Task<IActionResult> Handle(Controller controller, UserJourneyMissingContentException exception)
    {
        _logger.LogError(exception, "Handling errored user journey for section {Section}", exception.Section.Name);

        await _deleteCurrentSubmissionCommand.DeleteCurrentSubmission(exception.Section, cancellationToken);

        controller.TempData[ErrorMessageTempDataKey] = _configuration[ErrorMessageConfigKey];

        return controller.RedirectToAction(PagesController.GetPageByRouteAction, PagesController.ControllerName, new { route = RouteConstants.Home });
    }
}

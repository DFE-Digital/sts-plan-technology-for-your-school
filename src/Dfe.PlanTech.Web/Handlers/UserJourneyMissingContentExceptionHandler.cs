using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Handlers;

public class UserJourneyMissingContentExceptionHandler(
    ILoggerFactory loggerFactory,
    IConfiguration configuration,
    CurrentUser currentUser,
    SubmissionService submissionService
) : IUserJourneyMissingContentExceptionHandler
{
    private readonly ILogger<UserJourneyMissingContentExceptionHandler> _logger = loggerFactory.CreateLogger<UserJourneyMissingContentExceptionHandler>();
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly CurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    public const string ErrorMessageConfigKey = "ErrorMessages:ConcurrentUsersOrContentChange";
    public const string ErrorMessageTempDataKey = "SubtopicError";

    public async Task<IActionResult> Handle(Controller controller, UserJourneyMissingContentException exception)
    {
        _logger.LogError(exception, "Handling errored user journey for section {Section}", exception.Section.Name);

        var establishmentId = _currentUser.EstablishmentId
            ?? throw new InvalidDataException($"Current user has no {nameof(_currentUser.EstablishmentId)}");

        await _submissionService.DeleteCurrentSubmissionHardAsync(establishmentId, exception.Section.Id);

        controller.TempData[ErrorMessageTempDataKey] = _configuration[ErrorMessageConfigKey];

        return controller.RedirectToAction(PagesController.GetPageByRouteAction, PagesController.ControllerName, new { route = UrlConstants.Home });
    }
}

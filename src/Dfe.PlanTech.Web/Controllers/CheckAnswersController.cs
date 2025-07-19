using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Handlers;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Authorize]
[Route("/")]
public class CheckAnswersController: BaseController<CheckAnswersController>
{
    public const string ControllerName = "CheckAnswers";
    public const string CheckAnswersAction = nameof(CheckAnswersPage);
    public const string CheckAnswersPageSlug = "check-answers";
    public const string CheckAnswersViewName = "CheckAnswers";


    IUserJourneyMissingContentExceptionHandler _userJourneyMissingContentExceptionHandler;
    private readonly ReviewAnswersViewBuilder _reviewAnswersViewBuilder;

    public CheckAnswersController(
          ILogger<CheckAnswersController> logger,
          IUserJourneyMissingContentExceptionHandler userJourneyMissingContentExceptionHandler,
          ReviewAnswersViewBuilder reviewAnswersViewBuilder
    ) : base(logger)
    {
        _userJourneyMissingContentExceptionHandler = userJourneyMissingContentExceptionHandler ?? throw new ArgumentNullException(nameof(userJourneyMissingContentExceptionHandler));
        _reviewAnswersViewBuilder = reviewAnswersViewBuilder ?? throw new ArgumentNullException(nameof(reviewAnswersViewBuilder));
    }

    [HttpGet("{sectionSlug}/check-answers")]
    public async Task<IActionResult> CheckAnswersPage(string sectionSlug, [FromQuery] bool isChangeAnswersFlow)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug);

        try
        {
            var errorMessage = TempData["ErrorMessage"]?.ToString();
            return await _reviewAnswersViewBuilder.RouteBasedOnSubmissionStatus(this, sectionSlug, isChangeAnswersFlow, errorMessage);
        }
        catch (UserJourneyMissingContentException userJourneyException)
        {
            return await _userJourneyMissingContentExceptionHandler.Handle(this, userJourneyException);
        }
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(
        string sectionSlug,
        int submissionId,
        string sectionName,
        string redirectOption
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(submissionId);
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName);

        try
        {
            return await _reviewAnswersViewBuilder.ConfirmCheckAnswers(this, sectionSlug, sectionName, submissionId, redirectOption);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occurred while confirming a user's answers");
            throw;
        }
    }
}

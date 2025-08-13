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
public class CheckAnswersController(
    ILoggerFactory loggerFactory,
    IUserJourneyMissingContentExceptionHandler userJourneyMissingContentExceptionHandler,
    ReviewAnswersViewBuilder reviewAnswersViewBuilder
) : BaseController<CheckAnswersController>(loggerFactory)
{
    public const string ControllerName = "CheckAnswers";
    public const string CheckAnswersAction = nameof(CheckAnswersPage);
    public const string CheckAnswersPageSlug = "check-answers";
    public const string CheckAnswersViewName = "CheckAnswers";


    IUserJourneyMissingContentExceptionHandler _userJourneyMissingContentExceptionHandler = userJourneyMissingContentExceptionHandler ?? throw new ArgumentNullException(nameof(userJourneyMissingContentExceptionHandler));
    private readonly ReviewAnswersViewBuilder _reviewAnswersViewBuilder = reviewAnswersViewBuilder ?? throw new ArgumentNullException(nameof(reviewAnswersViewBuilder));

    [HttpGet("{sectionSlug}/check-answers")]
    public async Task<IActionResult> CheckAnswersPage(string categorySlug, string sectionSlug, [FromQuery] bool isChangeAnswersFlow)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(categorySlug, nameof(categorySlug));
        ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug, nameof(sectionSlug));

        try
        {
            var errorMessage = TempData["ErrorMessage"]?.ToString();
            return await _reviewAnswersViewBuilder.RouteBasedOnSubmissionStatus(this, categorySlug, sectionSlug, isChangeAnswersFlow, errorMessage);
        }
        catch (UserJourneyMissingContentException userJourneyException)
        {
            return await _userJourneyMissingContentExceptionHandler.Handle(this, userJourneyException);
        }
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(
        string categorySlug,
        string sectionSlug,
        int submissionId,
        string sectionName,
        string redirectOption
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(submissionId);
        ArgumentNullException.ThrowIfNullOrEmpty(categorySlug, nameof(categorySlug));
        ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug, nameof(sectionSlug));
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));

        try
        {
            return await _reviewAnswersViewBuilder.ConfirmCheckAnswers(this, categorySlug, sectionSlug, sectionName, submissionId, redirectOption);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occurred while confirming a user's answers");
            throw;
        }
    }
}

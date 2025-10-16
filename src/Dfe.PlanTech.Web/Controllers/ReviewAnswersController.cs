using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Handlers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Authorize]
[Route("/")]
public class ReviewAnswersController(
    ILogger<ReviewAnswersController> logger,
    IUserJourneyMissingContentExceptionHandler userJourneyMissingContentExceptionHandler,
    IReviewAnswersViewBuilder reviewAnswersViewBuilder
) : BaseController<ReviewAnswersController>(logger)
{
    private readonly IUserJourneyMissingContentExceptionHandler _userJourneyMissingContentExceptionHandler = userJourneyMissingContentExceptionHandler ?? throw new ArgumentNullException(nameof(userJourneyMissingContentExceptionHandler));
    private readonly IReviewAnswersViewBuilder _reviewAnswersViewBuilder = reviewAnswersViewBuilder ?? throw new ArgumentNullException(nameof(reviewAnswersViewBuilder));

    [HttpGet($"{{categorySlug}}/{{sectionSlug}}/{UrlConstants.CheckAnswersSlug}", Name = UrlConstants.CheckAnswersSlug)]
    public async Task<IActionResult> CheckAnswers(string categorySlug, string sectionSlug, [FromQuery] bool? isChangeAnswersFlow)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(categorySlug, nameof(categorySlug));
        ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug, nameof(sectionSlug));

        try
        {
            var errorMessage = TempData["ErrorMessage"]?.ToString();
            return await _reviewAnswersViewBuilder.RouteToCheckAnswers(this, categorySlug, sectionSlug, isChangeAnswersFlow, errorMessage);
        }
        catch (UserJourneyMissingContentException userJourneyException)
        {
            return await _userJourneyMissingContentExceptionHandler.Handle(this, userJourneyException);
        }
    }

    [HttpGet($"{{categorySlug}}/{{sectionSlug}}/{UrlConstants.ChangeAnswersSlug}")]
    public async Task<IActionResult> ChangeAnswers(string categorySlug, string sectionSlug)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(categorySlug, nameof(categorySlug));
        ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug, nameof(sectionSlug));

        try
        {
            var errorMessage = TempData["ErrorMessage"]?.ToString();
            return await _reviewAnswersViewBuilder.RouteToChangeAnswers(this, categorySlug, sectionSlug, errorMessage);
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
        string sectionName,
        int submissionId
    )
    {
        ArgumentNullException.ThrowIfNullOrEmpty(categorySlug, nameof(categorySlug));
        ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug, nameof(sectionSlug));
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(submissionId);

        try
        {
            TempData["SectionName"] = sectionName;
            return await _reviewAnswersViewBuilder.ConfirmCheckAnswers(this, categorySlug, sectionSlug, sectionName, submissionId);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occurred while confirming a user's answers");
            throw;
        }
    }
}

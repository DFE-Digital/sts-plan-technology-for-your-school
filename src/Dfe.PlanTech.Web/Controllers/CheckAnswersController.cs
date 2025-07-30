using Dfe.PlanTech.Domain.Exceptions;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Authorize]
[Route("/")]
public class CheckAnswersController(ILogger<CheckAnswersController> checkAnswersLogger) : BaseController<CheckAnswersController>(checkAnswersLogger)
{
    public const string ControllerName = "CheckAnswers";
    public const string CheckAnswersAction = nameof(CheckAnswersPage);
    public const string CheckAnswersPageSlug = "check-answers";
    public const string CheckAnswersViewName = "CheckAnswers";
    public const string RedirectToCategoryLandingPageAction = "RedirectToCategoryLandingPage";


    public const string InlineRecommendationUnavailableErrorMessage = "Unable to save. Please try again. If this problem continues you can";

    [HttpGet("{categorySlug}/{sectionSlug}/check-answers")]
    public async Task<IActionResult> CheckAnswersPage(string categorySlug,
                                                      string sectionSlug,
                                                      [FromQuery] bool isChangeAnswersFlow,
                                                      [FromServices] ICheckAnswersRouter checkAnswersValidator,
                                                      [FromServices] IUserJourneyMissingContentExceptionHandler userJourneyMissingContentExceptionHandler,
                                                      CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNullOrEmpty(categorySlug);
            ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug);

            var errorMessage = TempData["ErrorMessage"]?.ToString();

            return await checkAnswersValidator.ValidateRoute(categorySlug, sectionSlug, errorMessage, this, isChangeAnswersFlow, cancellationToken);
        }
        catch (UserJourneyMissingContentException userJourneyException)
        {
            return await userJourneyMissingContentExceptionHandler.Handle(this, userJourneyException, cancellationToken);
        }
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(
        string categorySlug,
        string sectionSlug,
        int submissionId,
        string sectionName,
        [FromServices] ICalculateMaturityCommand calculateMaturityCommand,
        [FromServices] IGetRecommendationRouter getRecommendationRouter,
        [FromServices] IMarkSubmissionAsReviewedCommand markSubmissionAsReviewedCommand,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(submissionId);
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName);

        try
        {
            await calculateMaturityCommand.CalculateMaturityAsync(submissionId, cancellationToken);
            // TODO: move this action to after the landing page has been revisited
            await markSubmissionAsReviewedCommand.MarkSubmissionAsReviewed(submissionId, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "There has been an error while trying to calculate maturity");
            TempData["ErrorMessage"] = InlineRecommendationUnavailableErrorMessage;
            return this.RedirectToCheckAnswers(categorySlug, sectionSlug);
        }

        return this.RedirectToCategoryLandingPage(categorySlug);
    }
}

using Dfe.PlanTech.Application.Constants;
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

    public const string InlineRecommendationUnavailableErrorMessage = "Unable to save. Please try again. If this problem continues you can";

    [HttpGet("{sectionSlug}/check-answers")]
    public async Task<IActionResult> CheckAnswersPage(string sectionSlug,
                                                      [FromServices] ICheckAnswersRouter checkAnswersValidator,
                                                      [FromServices] IUserJourneyMissingContentExceptionHandler userJourneyMissingContentExceptionHandler,
                                                      CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug);

            var errorMessage = TempData["ErrorMessage"]?.ToString();

            return await checkAnswersValidator.ValidateRoute(sectionSlug, errorMessage, this, cancellationToken);
        }
        catch (UserJourneyMissingContentException userJourneyException)
        {
            return await userJourneyMissingContentExceptionHandler.Handle(this, userJourneyException, cancellationToken);
        }
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(
        string sectionSlug,
        int submissionId,
        string sectionName,
        string redirectOption,
        [FromServices] ICalculateMaturityCommand calculateMaturityCommand,
        [FromServices] IGetRecommendationRouter getRecommendationRouter,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(submissionId);
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName);

        try
        {
            await calculateMaturityCommand.CalculateMaturityAsync(submissionId, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "There has been an error while trying to calculate maturity");
            TempData["ErrorMessage"] = InlineRecommendationUnavailableErrorMessage;
            return this.RedirectToCheckAnswers(sectionSlug);
        }

        return redirectOption switch
        {
            RecommendationsController.GetRecommendationAction => RedirectToAction(
                RecommendationsController.GetRecommendationAction,
                RecommendationsController.ControllerName,
                new {
                    sectionSlug,
                    recommendationSlug = await getRecommendationRouter.GetRecommendationSlugForSection(sectionSlug, cancellationToken)
                }),
            UrlConstants.SelfAssessmentPage => this.RedirectToSelfAssessment(),
            _ => this.RedirectToCheckAnswers(sectionSlug)
        };
    }
}

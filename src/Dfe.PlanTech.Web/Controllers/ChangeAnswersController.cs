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
public class ChangeAnswersController : BaseController<ChangeAnswersController>
{
    public const string ControllerName = "ChangeAnswers";
    public const string ChangeAnswersAction = nameof(ChangeAnswersPage);
    public const string ChangeAnswersPageSlug = "change-answers";
    public const string ChangeAnswersViewName = "ChangeAnswers";

    public const string InlineRecommendationUnavailableErrorMessage = "Unable to save. Please try again. If this problem continues you can";

    IUserJourneyMissingContentExceptionHandler _userJourneyMissingContentExceptionHandler;
    private readonly ReviewAnswersViewBuilder _reviewAnswersViewBuilder;

    public ChangeAnswersController(
        ILoggerFactory loggerFactory,
        IUserJourneyMissingContentExceptionHandler userJourneyMissingContentExceptionHandler,
        ReviewAnswersViewBuilder reviewAnswersViewBuilder
    ) : base(loggerFactory)
    {
        _userJourneyMissingContentExceptionHandler = userJourneyMissingContentExceptionHandler ?? throw new ArgumentNullException(nameof(userJourneyMissingContentExceptionHandler));
        _reviewAnswersViewBuilder = reviewAnswersViewBuilder ?? throw new ArgumentNullException(nameof(reviewAnswersViewBuilder));
    }

    [HttpGet("{categorySlug}/{sectionSlug}/change-answers")]
    public async Task<IActionResult> ChangeAnswersPage(string categorySlug, string sectionSlug)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(categorySlug, nameof(categorySlug));
        ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug, nameof(sectionSlug));

        try
        {
            var errorMessage = TempData["ErrorMessage"]?.ToString();
            return await _reviewAnswersViewBuilder.RouteBasedOnSubmissionStatus(this, categorySlug, sectionSlug, false, errorMessage);
        }
        catch (UserJourneyMissingContentException userJourneyException)
        {
            return await _userJourneyMissingContentExceptionHandler.Handle(this, userJourneyException);
        }
    }
}

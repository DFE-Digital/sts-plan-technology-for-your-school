using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Handlers;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers
{
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
        private readonly CurrentUser _currentUser;
        private readonly ChangeAnswersViewBuilder _viewBuilder;
        private readonly SubmissionService _submissionService;

        public ChangeAnswersController(
            ILogger<ChangeAnswersController> logger,
            IUserJourneyMissingContentExceptionHandler userJourneyMissingContentExceptionHandler,
            ChangeAnswersViewBuilder viewBuilder,
            CurrentUser currentUser,
            SubmissionService submissionService
        ) : base(logger)
        {
            _userJourneyMissingContentExceptionHandler = userJourneyMissingContentExceptionHandler ?? throw new ArgumentNullException(nameof(userJourneyMissingContentExceptionHandler));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _viewBuilder = viewBuilder ?? throw new ArgumentNullException(nameof(viewBuilder));
            _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));
        }

        [HttpGet("{sectionSlug}/change-answers")]
        public async Task<IActionResult> ChangeAnswersPage(
            string sectionSlug,
            [FromServices] )
        {
            ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug);

            try
            {
                return await _viewBuilder.RouteBasedOnSubmissionStatus(this, sectionSlug, TempData["ErrorMessage"]?.ToString());
            }
            catch (UserJourneyMissingContentException userJourneyException)
            {
                return await _userJourneyMissingContentExceptionHandler.Handle(this, userJourneyException);
            }
        }

        [HttpGet("recommendations/from-section/{sectionSlug}")]
        public async Task<IActionResult> RedirectToRecommendation(string sectionSlug)
        {
            var recommendationSlug = await getRecommendationRouter.GetRecommendationSlugForSection(sectionSlug);

            return RedirectToAction(
                  RecommendationsController.GetRecommendationAction,
                  RecommendationsController.ControllerName,
                  new { sectionSlug, recommendationSlug }
              );
        }
    }
}

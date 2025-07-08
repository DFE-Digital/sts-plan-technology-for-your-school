using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Handlers;
using Dfe.PlanTech.Web.Routing;
using Dfe.PlanTech.Web.Services;
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

        private readonly CurrentUser _currentUser;
        private readonly Router _router;
        private readonly SubmissionService _submissionService;

        public ChangeAnswersController(
            ILogger<ChangeAnswersController> logger,
            CurrentUser currentUser,
            Router router,
            SubmissionService submissionService
        ) : base(logger)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));
        }

        [HttpGet("{sectionSlug}/change-answers")]
        public async Task<IActionResult> ChangeAnswersPage(
            string sectionSlug,
            [FromServices] IChangeAnswersRouter changeAnswersValidator,
            [FromServices] IUserJourneyMissingContentExceptionHandler userJourneyMissingContentExceptionHandler)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug);

            try
            {
                return await _router.RouteToNextPage(this, sectionSlug, isCompleted: true, TempData["ErrorMessage"]?.ToString());
            }
            catch (UserJourneyMissingContentException userJourneyException)
            {
                return await userJourneyMissingContentExceptionHandler.Handle(this, userJourneyException);
            }
        }

        [HttpGet("recommendations/from-section/{sectionSlug}")]
        public async Task<IActionResult> RedirectToRecommendation(
            string sectionSlug,
            [FromServices] IGetRecommendationRouter getRecommendationRouter,
            CancellationToken cancellationToken = default)
        {
            var recommendationSlug = await getRecommendationRouter.GetRecommendationSlugForSection(sectionSlug, cancellationToken);

            return RedirectToAction(
                  RecommendationsController.GetRecommendationAction,
                  RecommendationsController.ControllerName,
                  new { sectionSlug, recommendationSlug }
              );
        }
    }
}

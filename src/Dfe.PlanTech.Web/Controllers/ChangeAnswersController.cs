using Dfe.PlanTech.Domain.Exceptions;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Middleware;
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

        private readonly IUser _user;

        public ChangeAnswersController(
            ILogger<ChangeAnswersController> logger,
            IUser user
        ) : base(logger)
        {
            _user = user;
        }

        [HttpGet("{categorySlug}/{sectionSlug}/change-answers")]
        public async Task<IActionResult> ChangeAnswersPage(
            string categorySlug,
            string sectionSlug,
            [FromServices] IChangeAnswersRouter changeAnswersValidator,
            [FromServices] IUserJourneyMissingContentExceptionHandler userJourneyMissingContentExceptionHandler,
            CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNullOrEmpty(categorySlug);
                ArgumentNullException.ThrowIfNullOrEmpty(sectionSlug);

                var errorMessage = TempData["ErrorMessage"]?.ToString();

                return await changeAnswersValidator.ValidateRoute(categorySlug, sectionSlug, errorMessage, this, cancellationToken);
            }
            catch (UserJourneyMissingContentException userJourneyException)
            {
                return await userJourneyMissingContentExceptionHandler.Handle(this, userJourneyException, cancellationToken);
            }
        }
    }
}

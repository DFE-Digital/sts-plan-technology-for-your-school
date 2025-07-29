using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing
{
    public class ChangeAnswersRouter : IChangeAnswersRouter
    {
        private const string PageTitle = "Change Answers";

        private readonly IGetPageQuery _getPageQuery;
        private readonly IProcessSubmissionResponsesCommand _processSubmissionResponsesCommand;
        private readonly IUser _user;
        private readonly ISubmissionStatusProcessor _router;
        private readonly IGetLatestResponsesQuery _submissionQuery;
        private readonly ISubmissionCommand _submissionCommand;

        public ChangeAnswersRouter(
            IGetPageQuery getPageQuery,
            IProcessSubmissionResponsesCommand processSubmissionResponsesCommand,
            IUser user,
            ISubmissionStatusProcessor router,
            IGetLatestResponsesQuery submissionQuery,
            ISubmissionCommand submissionCommand)
        {
            _getPageQuery = getPageQuery;
            _processSubmissionResponsesCommand = processSubmissionResponsesCommand;
            _user = user;
            _router = router;
            _submissionQuery = submissionQuery;
            _submissionCommand = submissionCommand;
        }

        public async Task<IActionResult> ValidateRoute(
            string categorySlug,
            string sectionSlug,
            string? errorMessage,
            ChangeAnswersController controller,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(sectionSlug))
                throw new ArgumentNullException(nameof(sectionSlug));

            await _router.GetJourneyStatusForSectionRecommendation(sectionSlug, true, cancellationToken);

            switch (_router.Status)
            {
                case Status.CompleteNotReviewed:
                    return await ProcessChangeAnswers(categorySlug, sectionSlug, errorMessage, controller, cancellationToken);

                case Status.CompleteReviewed:
                {
                    var establishmentId = await _router.User.GetEstablishmentId();
                    var sectionId = _router.Section.Sys.Id;

                    // Check if an in-progress submission already exists
                    var inProgressSubmission = await _submissionQuery.GetInProgressSubmission(
                        establishmentId, sectionId, cancellationToken);

                    if (inProgressSubmission != null)
                    {
                        await _submissionCommand.DeleteSubmission(inProgressSubmission.Id, cancellationToken);
                    }

                    var latestSubmission = await _submissionQuery.GetLatestCompletedSubmission(
                                                        establishmentId, sectionId);

                    var clonedSubmission = await _submissionCommand.CloneSubmission(
                        latestSubmission, cancellationToken);

                    return await ProcessChangeAnswers(categorySlug, sectionSlug, errorMessage, controller, cancellationToken);
                }

                case Status.NotStarted:
                    return PageRedirecter.RedirectToHomepage(controller);

                default:
                    return ProcessQuestionStatus(sectionSlug, controller);
            }
        }

        private async Task<IActionResult> ProcessChangeAnswers(
            string categorySlug,
            string sectionSlug,
            string? errorMessage,
            ChangeAnswersController controller,
            CancellationToken cancellationToken)
        {
            var establishmentId = await _user.GetEstablishmentId();

            var submissionResponsesDto = await _processSubmissionResponsesCommand
                .GetSubmissionResponsesDtoForSection(establishmentId, _router.Section, true, cancellationToken);

            await _router.GetJourneyStatusForSectionRecommendation(sectionSlug, false, cancellationToken);

            if (submissionResponsesDto?.Responses == null)
                throw new DatabaseException("Could not retrieve the answered question list");

            var model = new ChangeAnswersViewModel()
            {
                Title = new Title() { Text = PageTitle },
                SectionName = _router.Section.Name,
                SubmissionResponses = submissionResponsesDto,
                SectionSlug = sectionSlug,
                SubmissionId = submissionResponsesDto.SubmissionId,
                Slug = ChangeAnswersController.ChangeAnswersPageSlug,
                ErrorMessage = errorMessage,
                CategorySlug = categorySlug
            };

            return controller.View(ChangeAnswersController.ChangeAnswersViewName, model);
        }

        private IActionResult ProcessQuestionStatus(string sectionSlug, Controller controller)
        {
            var nextQuestionSlug = _router.NextQuestion?.Slug ?? _router.Section.Questions.First().Slug;
            return QuestionsController.RedirectToQuestionBySlug(sectionSlug, nextQuestionSlug, controller);
        }
    }
}

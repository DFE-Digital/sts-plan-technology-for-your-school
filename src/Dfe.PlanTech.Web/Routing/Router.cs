using System.Configuration;
using Dfe.PlanTech.Application.Context;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Services;
using Dfe.PlanTech.Web.Workflows;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp;

namespace Dfe.PlanTech.Web.Routing
{
    public class Router(
        CurrentUser currentUser,
        SubmissionService submissionService
    )
    {
        private readonly CurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        private readonly SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

        public async Task<IActionResult> RouteToNextPage(Controller controller, string sectionSlug, bool isCompleted, string? errorMessage = null)
        {
            var submissionInformation = await _submissionService.GetSubmissionInformationAsync(sectionSlug, isCompleted);

            var model = BuildChangeAnswersViewModel(submissionInformation, sectionSlug, errorMessage);
            switch (submissionInformation.SectionStatus.Status)
            {
                case SubmissionStatus.NotStarted:
                    return PageRedirecter.RedirectToSelfAssessment(controller);

                case SubmissionStatus.InProgress:

                    break;

                case SubmissionStatus.CompleteNotReviewed:
                    return controller.View(ChangeAnswersController.ChangeAnswersViewName, model);

                case SubmissionStatus.CompleteReviewed:
                    await _submissionService.RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(submissionInformation.EstablishmentId, submissionInformation.Section.Sys.Id);
                    return controller.View(ChangeAnswersController.ChangeAnswersViewName, model);

                case default:
                    return QuestionsController.RedirectToQuestionBySlug(sectionSlug, nextQuestionSlug, controller);

            }
        }

        public ChangeAnswersViewModel BuildChangeAnswersViewModel(SubmissionInformationModel submissionInformation, string sectionSlug, string? errorMessage)
        {
            return new ChangeAnswersViewModel()
            {
                Title = new CmsTitleDto { Text = PageTitleConstants.ChangeAnswers },
                SectionName = submissionInformation.Section.Name,
                SubmissionResponses = submissionInformation.SubmissionResponses,
                SectionSlug = sectionSlug,
                SubmissionId = submissionInformation.SubmissionResponses.SubmissionId,
                Slug = ChangeAnswersController.ChangeAnswersPageSlug,
                ErrorMessage = errorMessage
            };
        }
    }
}

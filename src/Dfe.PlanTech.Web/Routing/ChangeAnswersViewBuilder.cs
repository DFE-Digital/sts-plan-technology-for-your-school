using System.Configuration;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Application.Context;
using Dfe.PlanTech.Application.Controllers;
using Dfe.PlanTech.Application.Models;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;

namespace Dfe.PlanTech.Web.Routing
{
    public class ChangeAnswersViewBuilder(
        CurrentUser currentUser,
        ContentfulWorkflow contentfulWorkflow,
        SubmissionService submissionService
    )
    {
        private readonly CurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        private readonly ContentfulWorkflow _contentfulWorkflow = contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));
        private readonly SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

        public async Task<IActionResult> RouteBasedOnSubmissionStatus(Controller controller, string sectionSlug, string? errorMessage = null)
        {
            var establishmentId = _currentUser.EstablishmentId
                ?? throw new ArgumentNullException(nameof(currentUser.EstablishmentId));

            var sectionId = await _contentfulWorkflow.GetSectionBySlug(sectionSlug);

            var submissionInformation = await _submissionService.GetSubmissionResponsesForSection(establishmentId, sectionSlug);

            var model = BuildChangeAnswersViewModel(submissionInformation, sectionSlug, errorMessage);
            switch (submissionInformation.SectionStatus.Status)
            {
                case SubmissionStatus.NotStarted:
                    return controller.RedirectToSelfAssessment();

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

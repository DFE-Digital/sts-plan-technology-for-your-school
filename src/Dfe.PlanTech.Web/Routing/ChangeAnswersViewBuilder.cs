using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing
{
    public class ChangeAnswersViewBuilder(
        CurrentUser currentUser,
        SubmissionService submissionService
    )
    {
        private readonly CurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        private readonly SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

        public async Task<IActionResult> RouteBasedOnSubmissionStatus(Controller controller, string sectionSlug, string? errorMessage = null)
        {
            var establishmentId = _currentUser.EstablishmentId
                ?? throw new InvalidDataException(nameof(currentUser.EstablishmentId));

            var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, sectionSlug);

            var model = BuildChangeAnswersViewModel(submissionRoutingData, sectionSlug, errorMessage);
            switch (submissionRoutingData.Status)
            {
                case SubmissionStatus.NotStarted:
                    return controller.RedirectToSelfAssessment();

                case SubmissionStatus.CompleteNotReviewed:
                    return controller.View(ChangeAnswersController.ChangeAnswersViewName, model);

                case SubmissionStatus.CompleteReviewed:
                    await _submissionService.RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(establishmentId, submissionRoutingData.QuestionnaireSection.Sys.Id!);
                    return controller.View(ChangeAnswersController.ChangeAnswersViewName, model);

                default:
                    return QuestionsController.RedirectToQuestionBySlug(sectionSlug, submissionRoutingData.NextQuestion!.Slug, controller);

            }
        }

        public ChangeAnswersViewModel BuildChangeAnswersViewModel(SubmissionRoutingDataModel routingData, string sectionSlug, string? errorMessage)
        {
            return new ChangeAnswersViewModel()
            {
                Title = PageTitleConstants.ChangeAnswers,
                SectionName = routingData.QuestionnaireSection.Name,
                SubmissionResponses = BuildSubmissionResponsesViewModel(routingData.Submission),
                SectionSlug = sectionSlug,
                SubmissionId = routingData.Submission?.Id,
                Slug = ChangeAnswersController.ChangeAnswersPageSlug,
                ErrorMessage = errorMessage
            };
        }

        public SubmissionResponsesViewModel BuildSubmissionResponsesViewModel(SqlSubmissionDto? submission)
        {
            return new SubmissionResponsesViewModel
            {
                SubmissionId = submission?.Id,
                Responses = submission?.Responses.Select(BuildSubmissionResponseViewModel).ToList()
            };
        }

        public SubmissionResponseViewModel BuildSubmissionResponseViewModel(SqlResponseDto response)
        {
            var question = response.Question;
            var answer = response.Answer;

            return new SubmissionResponseViewModel
            {
                QuestionRef = question.ContentfulSysId,
                QuestionText = question.QuestionText,
                AnswerRef = answer.ContentfulSysId,
                AnswerText = answer.AnswerText,
                DateCreated = response.DateCreated,
            };
        }
    }
}

using Dfe.PlanTech.Application.Context;
using Dfe.PlanTech.Application.Controllers;
using Dfe.PlanTech.Application.Models;
using Dfe.PlanTech.Application.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public class ChangeAnswersRouter : IChangeAnswersRouter
{
    private readonly CurrentUser _currentUser;

    public ChangeAnswersRouter(
        CurrentUser currentUser
    )
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<IActionResult> ValidateRoute(string sectionSlug, string? errorMessage)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        await _router.GetJourneyStatusForSectionRecommendation(sectionSlug, isCompleted: true);

        switch (_router.Status)
        {
            case Status.CompleteNotReviewed:
                return await ProcessChangeAnswers(sectionSlug, errorMessage);

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

                return await ProcessChangeAnswers(sectionSlug, errorMessage, controller, cancellationToken);
            }

            case Status.NotStarted:
                return PageRedirecter.RedirectToSelfAssessment(controller);

            default:
                return ProcessQuestionStatus(sectionSlug, controller);
        }
    }

    private async Task<IActionResult> ProcessChangeAnswers(
        string sectionSlug,
        string? errorMessage)
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
            ErrorMessage = errorMessage
        };

        return controller.View(ChangeAnswersController.ChangeAnswersViewName, model);
    }

    private IActionResult ProcessQuestionStatus(string sectionSlug, Controller controller)
    {
        var nextQuestionSlug = _router.NextQuestion?.Slug ?? _router.Section.Questions.First().Slug;
        return QuestionsController.RedirectToQuestionBySlug(sectionSlug, nextQuestionSlug, controller);
    }
}

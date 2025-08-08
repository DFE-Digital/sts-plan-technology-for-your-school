using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public class CheckAnswersRouter : ICheckAnswersRouter
{
    private const string PageTitle = "Check Answers";

    private readonly IGetPageQuery _getPageQuery;
    private readonly IProcessSubmissionResponsesCommand _processSubmissionResponsesCommand;
    private readonly IUser _user;
    private readonly ISubmissionStatusProcessor _router;

    public CheckAnswersRouter(IGetPageQuery getPageQuery,
                              IProcessSubmissionResponsesCommand processSubmissionResponsesCommand,
                              IUser user,
                              ISubmissionStatusProcessor router)
    {
        _getPageQuery = getPageQuery;
        _processSubmissionResponsesCommand = processSubmissionResponsesCommand;
        _user = user;
        _router = router;
    }

    public async Task<IActionResult> ValidateRoute(string categorySlug, string sectionSlug, string? errorMessage, CheckAnswersController controller, bool isChangeAnswersFlow = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(categorySlug))
            throw new ArgumentNullException(nameof(categorySlug));
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        await _router.GetJourneyStatusForSection(sectionSlug, isChangeAnswersFlow, cancellationToken);

        return _router.Status switch
        {
            Status.CompleteNotReviewed => await ProcessCheckAnswers(categorySlug, sectionSlug, errorMessage, controller, cancellationToken),
            Status.CompleteReviewed when isChangeAnswersFlow => await ProcessCheckAnswers(categorySlug, sectionSlug, errorMessage, controller, cancellationToken),
            Status.NotStarted => PageRedirecter.RedirectToHomepage(controller),
            _ => ProcessQuestionStatus(sectionSlug, controller),
        };
    }

    private async Task<IActionResult> ProcessCheckAnswers(string categorySlug, string sectionSlug, string? errorMessage, CheckAnswersController controller, CancellationToken cancellationToken)
    {
        var establishmentId = await _user.GetEstablishmentId();

        var submissionResponsesDto = await _processSubmissionResponsesCommand.GetSubmissionResponsesDtoForSection(establishmentId, _router.Section, cancellationToken: cancellationToken);

        if (submissionResponsesDto == null || submissionResponsesDto.Responses == null)
            throw new DatabaseException("Could not retrieve the answered question list");

        var checkAnswerPageContent = await _getPageQuery.GetPageBySlug(CheckAnswersController.CheckAnswersPageSlug, cancellationToken);

        var model = new CheckAnswersViewModel()
        {
            Title = checkAnswerPageContent!.Title ?? new Title() { Text = PageTitle },
            SectionName = _router.Section.Name,
            SubmissionResponses = submissionResponsesDto,
            Content = checkAnswerPageContent.Content,
            SectionSlug = sectionSlug,
            SubmissionId = submissionResponsesDto.SubmissionId,
            Slug = checkAnswerPageContent.Slug,
            ErrorMessage = errorMessage,
            CategorySlug = categorySlug
        };

        return controller.View(CheckAnswersController.CheckAnswersViewName, model);
    }

    private IActionResult ProcessQuestionStatus(string sectionSlug, Controller controller)
    {
        var nextQuestionSlug = _router.NextQuestion?.Slug ?? _router.Section.Questions.Select(question => question.Slug).First();

        return QuestionsController.RedirectToQuestionBySlug(sectionSlug, nextQuestionSlug, controller);
    }
}

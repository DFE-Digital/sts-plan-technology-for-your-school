using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Exceptions;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public class CheckAnswersRouter : ICheckAnswersRouter
{
    private const string PageTitle = "Check Answers";

    private readonly IGetPageQuery _getPageQuery;
    private readonly IProcessCheckAnswerDtoCommand _processCheckAnswerDtoCommand;
    private readonly IUser _user;
    private readonly ISubmissionStatusProcessor _router;

    public CheckAnswersRouter(IGetPageQuery getPageQuery,
                              IProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand,
                              IUser user,
                              ISubmissionStatusProcessor router)
    {
        _getPageQuery = getPageQuery;
        _processCheckAnswerDtoCommand = processCheckAnswerDtoCommand;
        _user = user;
        _router = router;
    }

    public async Task<IActionResult> ValidateRoute(string sectionSlug, string? errorMessage, CheckAnswersController controller, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));

        await _router.GetJourneyStatusForSection(sectionSlug, cancellationToken);

        return _router.Status switch
        {
            SubmissionStatus.CheckAnswers => await ProcessCheckAnswers(sectionSlug, errorMessage, controller, cancellationToken),
            SubmissionStatus.Completed or SubmissionStatus.NotStarted => PageRedirecter.RedirectToSelfAssessment(controller),
            _ => ProcessQuestionStatus(sectionSlug, controller),
        };
    }

    private async Task<IActionResult> ProcessCheckAnswers(string sectionSlug, string? errorMessage, CheckAnswersController controller, CancellationToken cancellationToken)
    {
        var establishmentId = await _user.GetEstablishmentId();
        var checkAnswerDto = await _processCheckAnswerDtoCommand.GetCheckAnswerDtoForSection(establishmentId, _router.Section, cancellationToken);

        if (checkAnswerDto == null || checkAnswerDto.Responses == null) throw new DatabaseException("Could not retrieve the answered question list");

        var checkAnswerPageContent = await _getPageQuery.GetPageBySlug(CheckAnswersController.CheckAnswersPageSlug, cancellationToken) ??
                                    throw new PageNotFoundException($"Could not find page for {CheckAnswersController.CheckAnswersPageSlug}");

        var model = new CheckAnswersViewModel()
        {
            Title = checkAnswerPageContent!.Title ?? new Title() { Text = PageTitle },
            SectionName = _router.Section.Name,
            CheckAnswerDto = checkAnswerDto,
            Content = checkAnswerPageContent.Content,
            SectionSlug = sectionSlug,
            SubmissionId = checkAnswerDto.SubmissionId,
            Slug = checkAnswerPageContent.Slug,
            ErrorMessage = errorMessage
        };

        return controller.View(CheckAnswersController.CheckAnswersViewName, model);
    }

    private IActionResult ProcessQuestionStatus(string sectionSlug, Controller controller)
    {
        var nextQuestionSlug = _router.NextQuestion?.Slug ?? _router.Section.Questions.Select(question => question.Slug).First();

        return QuestionsController.RedirectToQuestionBySlug(sectionSlug, nextQuestionSlug, controller);
    }
}
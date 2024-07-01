using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
[Route("/")]
public class QuestionsController : BaseController<QuestionsController>
{
    public const string Controller = "Questions";
    public const string GetQuestionBySlugActionName = nameof(GetQuestionBySlug);

    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IGetLatestResponsesQuery _getResponseQuery;
    private readonly IUser _user;

    public QuestionsController(ILogger<QuestionsController> logger,
                               IGetSectionQuery getSectionQuery,
                               IGetLatestResponsesQuery getResponseQuery,
                               IUser user) : base(logger)
    {
        _getResponseQuery = getResponseQuery;
        _getSectionQuery = getSectionQuery;
        _user = user;
    }

    [LogInvalidModelState]
    [HttpGet("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> GetQuestionBySlug(string sectionSlug,
                                                        string questionSlug,
                                                        [FromServices] IGetQuestionBySlugRouter router,
                                                        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(questionSlug)) throw new ArgumentNullException(nameof(questionSlug));

        return await router.ValidateRoute(sectionSlug, questionSlug, this, cancellationToken);
    }


    [LogInvalidModelState]
    [HttpGet("{sectionSlug}/next-question")]
    public async Task<IActionResult> GetNextUnansweredQuestion(string sectionSlug,
                                                                [FromServices] IGetNextUnansweredQuestionQuery getQuestionQuery,
                                                                [FromServices] IDeleteCurrentSubmissionCommand deleteCurrentSubmissionCommand,
                                                                CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));

        var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                        throw new ContentfulDataUnavailableException($"Could not find section with slug {sectionSlug}");

        int establishmentId = await _user.GetEstablishmentId();

        try
        {
            var nextQuestion =
                await getQuestionQuery.GetNextUnansweredQuestion(establishmentId, section, cancellationToken);

            if (nextQuestion == null) return this.RedirectToCheckAnswers(sectionSlug);

            return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug = nextQuestion!.Slug });
        }
        catch (DatabaseException)
        {
            // Remove the current invalid submission and redirect to self-assessment page
            await deleteCurrentSubmissionCommand.DeleteCurrentSubmission(section, cancellationToken);
            TempData["SubtopicError"] = "Sorry, there has been a problem with the service and your answers have not been saved. Please try again.";
            return RedirectToAction(
                PagesController.GetPageByRouteAction,
                PagesController.ControllerName,
                new { route = "self-assessment" });
        }
    }

    [HttpPost("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(string sectionSlug, string questionSlug, SubmitAnswerDto submitAnswerDto, [FromServices] ISubmitAnswerCommand submitAnswerCommand, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = await GenerateViewModel(sectionSlug, questionSlug, cancellationToken);
            viewModel.ErrorMessages = ModelState.Values.SelectMany(value => value.Errors.Select(err => err.ErrorMessage)).ToArray();
            return RenderView(viewModel);
        }

        try
        {
            await submitAnswerCommand.SubmitAnswer(submitAnswerDto, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError("An error has occurred while submitting an answer with the following message: {message} ", e.Message);
            var viewModel = await GenerateViewModel(sectionSlug, questionSlug, cancellationToken);
            viewModel.ErrorMessages = new[] { "Save failed. Please try again later." };
            return RenderView(viewModel);
        }

        return RedirectToAction(nameof(GetNextUnansweredQuestion), new { sectionSlug });
    }

    [NonAction]
    public IActionResult RenderView(QuestionViewModel viewModel) => View("Question", viewModel);

    [NonAction]
    private async Task<QuestionViewModel> GenerateViewModel(string sectionSlug, string questionSlug, CancellationToken cancellationToken)
    {
        var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                        throw new KeyNotFoundException($"Could not find section with slug {sectionSlug}");

        var question = section.Questions.FirstOrDefault(question => question.Slug == questionSlug) ??
                            throw new KeyNotFoundException($"Could not find question slug {questionSlug} under section {sectionSlug}");

        int establishmentId = await _user.GetEstablishmentId();

        var latestResponseForQuestion = await _getResponseQuery.GetLatestResponseForQuestion(establishmentId,
                                                                                section.Sys.Id,
                                                                                question.Sys.Id,
                                                                                cancellationToken);

        return GenerateViewModel(sectionSlug, question, section, latestResponseForQuestion?.AnswerRef);
    }

    [NonAction]
    public QuestionViewModel GenerateViewModel(string sectionSlug, Question question, ISectionComponent section, string? latestAnswerContentfulId)
    {
        ViewData["Title"] = question.Text;

        return new QuestionViewModel()
        {
            Question = question,
            AnswerRef = latestAnswerContentfulId,
            SectionName = section.Name,
            SectionSlug = sectionSlug,
            SectionId = section.Sys.Id
        };
    }

    public static IActionResult RedirectToQuestionBySlug(string sectionSlug, string questionSlug, Controller controller)
    => controller.RedirectToAction(GetQuestionBySlugActionName, Controller, new { sectionSlug, questionSlug });
}
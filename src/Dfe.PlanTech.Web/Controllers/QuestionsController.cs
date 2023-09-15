using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Constants;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class QuestionsController : BaseController<QuestionsController>
{
    private readonly GetQuestionQuery _getQuestionQuery;
    public QuestionsController(ILogger<QuestionsController> logger, GetQuestionQuery getQuestionQuery) : base(logger)
    {
        _getQuestionQuery = getQuestionQuery;
    }

    [HttpGet("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> GetQuestionBySlug(string sectionSlug, string questionSlug, CancellationToken cancellationToken)
    {
        //TODO: Check status -> redirect to check answers if appropriate
        var question = await _getQuestionQuery.GetQuestionBySlug(sectionSlug: sectionSlug, questionSlug: questionSlug, cancellationToken);

        var viewModel = new QuestionViewModel()
        {
            Question = question.Question,
            AnswerRef = null,
            Params = null,
            SubmissionId = 1,
            QuestionErrorMessage = null,
            SectionSlug = sectionSlug,
            SectionId = question.SectionId
        };

        return View("Question", viewModel);

    }

    [HttpPost("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(string sectionSlug, string questionSlug, SubmitAnswerDto submitAnswerDto, [FromServices] ISubmitAnswerCommand submitAnswerCommand)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug });

        var result = await submitAnswerCommand.SubmitAnswer(submitAnswerDto, submitAnswerDto.SectionId, sectionName: sectionSlug);

        if (submitAnswerDto.ChosenAnswer?.NextQuestion == null)
        {
            //TODO: Redirect to check answers page
            return Redirect("/self-assessment");
        }

        return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug = submitAnswerDto.ChosenAnswer.NextQuestion.Value.Slug });
    }
}
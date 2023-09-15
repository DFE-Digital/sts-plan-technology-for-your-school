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
    public async Task<IActionResult> GetQuestionBySlug(string sectionSlug, string questionSlug, [FromServices] ISubmitAnswerCommand submitAnswerCommand, CancellationToken cancellationToken)
    {
        var question = await _getQuestionQuery.GetQuestionBySlug(sectionSlug: sectionSlug, questionSlug: questionSlug, cancellationToken);

        /*
        var parameterQuestionPage = TempData[TempDataConstants.Questions] != null ? DeserialiseParameter<TempDataQuestions>(TempData[TempDataConstants.Questions]) : new TempDataQuestions();
        string id;

        if (TempData.Peek("questionId") is string questionId && !string.IsNullOrEmpty(questionId))
        {
            id = questionId;
            TempData["questionId"] = null;
        }
        else
        {
            id = parameterQuestionPage.QuestionRef;
        }

        TempData.TryGetValue("param", out object? parameters);
        Params? param = ParamParser._ParseParameters(parameters?.ToString());

        var questionWithSubmission = await submitAnswerCommand.GetQuestionWithSubmission(parameterQuestionPage.SubmissionId, id, param?.SectionId ?? throw new NullReferenceException(nameof(param)), section, cancellationToken);

        if (questionWithSubmission.Question == null)
        {
            TempData[TempDataConstants.CheckAnswers] = SerialiseParameter(new TempDataCheckAnswers() { SubmissionId = questionWithSubmission.Submission?.Id ?? throw new NullReferenceException(nameof(questionWithSubmission.Submission)), SectionId = param.SectionId, SectionName = param.SectionName, SectionSlug = param.SectionSlug });
            return RedirectToRoute("CheckAnswersRoute", new { sectionSlug = param.SectionSlug });
        }
        else
        {
        */
        var viewModel = new QuestionViewModel()
        {
            Question = question,
            AnswerRef = null,
            Params = null,
            SubmissionId = 1,
            QuestionErrorMessage = null,
            SectionSlug = sectionSlug
        };

        return View("Question", viewModel);

    }

    [HttpPost("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(string sectionSlug, string questionSlug, SubmitAnswerDto submitAnswerDto, [FromServices] ISubmitAnswerCommand submitAnswerCommand)
    {
        string? nextQuestionId;

        try
        {
            nextQuestionId = await submitAnswerCommand.GetNextQuestionId(submitAnswerDto.QuestionId, submitAnswerDto.ChosenAnswerId);
        }
        catch (Exception e)
        {
            logger.LogError("An error has occurred while retrieving the next question with the following message: {errorNoNextQuestionId} ", e.Message);
            return Redirect(UrlConstants.ServiceUnavailable);
        }


        if (string.IsNullOrEmpty(nextQuestionId))
        {
            return Redirect("/self-assessment");
        }
        else
        {
            return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug });
        }
    }
}
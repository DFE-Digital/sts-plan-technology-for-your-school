using Dfe.PlanTech.Application.Submission.Commands;
using Dfe.PlanTech.Domain.Questionnaire.Constants;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
[Route("/question")]
public class QuestionsController : BaseController<QuestionsController>
{
    public QuestionsController(ILogger<QuestionsController> logger) : base(logger) { }

    [HttpGet("{id?}")]
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="section">Name of current section (if starting new)</param>
    /// <returns></returns>
    public async Task<IActionResult> GetQuestionById(string? id, string? section, [FromServices] SubmitAnswerCommand submitAnswerCommand, CancellationToken cancellationToken)
    {
        var parameterQuestionPage = TempData[TempDataConstants.Questions] != null ? DeserialiseParameter<TempDataQuestions>(TempData[TempDataConstants.Questions]) : new TempDataQuestions();

        if (string.IsNullOrEmpty(id)) id = parameterQuestionPage.QuestionRef;

        TempData.TryGetValue("param", out object? parameters);
        Params? param = _ParseParameters(parameters?.ToString());

        var nextQuestionResult = await submitAnswerCommand.GetNextQuestion(parameterQuestionPage.SubmissionId, id, param?.SectionId ?? throw new NullReferenceException(nameof(param)), section, cancellationToken);

        if (nextQuestionResult.Question == null)
        {
            TempData[TempDataConstants.CheckAnswers] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataCheckAnswers() { SubmissionId = nextQuestionResult.Submission?.Id ?? throw new NullReferenceException(nameof(nextQuestionResult.Submission)), SectionId = param.SectionId, SectionName = param.SectionName });
            return RedirectToAction("CheckAnswersPage", "CheckAnswers");
        }
        else
        {
            var viewModel = new QuestionViewModel()
            {
                Question = nextQuestionResult.Question,
                AnswerRef = parameterQuestionPage.AnswerRef,
                Params = parameters?.ToString(),
                SubmissionId = nextQuestionResult.Submission == null ? parameterQuestionPage.SubmissionId : nextQuestionResult.Submission.Id
            };

            return View("Question", viewModel);
        }
    }

    [HttpPost("SubmitAnswer")]
    public async Task<IActionResult> SubmitAnswer(SubmitAnswerDto submitAnswerDto, [FromServices] SubmitAnswerCommand submitAnswerCommand)
    {
        if (submitAnswerDto == null) throw new ArgumentNullException(nameof(submitAnswerDto));

        Params param = new Params();
        if (!string.IsNullOrEmpty(submitAnswerDto.Params))
        {
            param = _ParseParameters(submitAnswerDto.Params) ?? null!;
            TempData["param"] = submitAnswerDto.Params;
        }

        if (!ModelState.IsValid)
        {
            TempData[TempDataConstants.Questions] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataQuestions() { QuestionRef = submitAnswerDto.QuestionId, SubmissionId = submitAnswerDto.SubmissionId });
            return RedirectToAction("GetQuestionById");
        }

        int submissionId = await submitAnswerCommand.SubmitAnswer(submitAnswerDto, param.SectionId, param.SectionName);
        string? nextQuestionId = await submitAnswerCommand.GetNextQuestionId(submitAnswerDto.QuestionId, submitAnswerDto.ChosenAnswerId);

        if (string.IsNullOrEmpty(nextQuestionId) || await submitAnswerCommand.NextQuestionIsAnswered(submissionId, nextQuestionId))
        {
            TempData[TempDataConstants.CheckAnswers] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataCheckAnswers() { SubmissionId = submissionId, SectionId = param.SectionId, SectionName = param.SectionName });
            return RedirectToAction("CheckAnswersPage", "CheckAnswers");
        }
        else
        {
            TempData[TempDataConstants.Questions] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataQuestions() { QuestionRef = nextQuestionId, SubmissionId = submissionId });
            return RedirectToAction("GetQuestionById");
        }
    }

    private static Params? _ParseParameters(string? parameters)
    {
        if (string.IsNullOrEmpty(parameters))
            return null;

        var splitParams = parameters.Split('+');

        if (splitParams is null)
            return null;

        return new Params
        {
            SectionName = splitParams.Length > 0 ? splitParams[0].ToString() : string.Empty,
            SectionId = splitParams.Length > 1 ? splitParams[1].ToString() : string.Empty,
        };
    }
}
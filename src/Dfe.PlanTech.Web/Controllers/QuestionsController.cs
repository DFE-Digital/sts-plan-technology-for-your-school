using Dfe.PlanTech.Application.Submission.Commands;
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
    /// <param name="query"></param>
    /// <exception cref="ArgumentNullException">Throws exception when Id is null or empty</exception>
    /// <returns></returns>
    public async Task<IActionResult> GetQuestionById(
        string? id,
        string? section,
        [FromServices] SubmitAnswerCommand submitAnswerCommand,
        CancellationToken cancellationToken)
    {
        int? submissionId = null;
        string? answerRef = null;

        if (string.IsNullOrEmpty(id))
        {
            var parameterQuestionPage = _DeserialiseParameter(TempData["QuestionPage"]);
            id = parameterQuestionPage.QuestionRef;
            submissionId = parameterQuestionPage.SubmissionId;
            answerRef = parameterQuestionPage.AnswerRef;
        }

        object? parameters;
        TempData.TryGetValue("param", out parameters);

        Question? question = null;

        if (submissionId == null)
        {
            Params? param = _ParseParameters(parameters?.ToString());
            var submission = await submitAnswerCommand.GetOngoingSubmission(param?.SectionId ?? throw new NullReferenceException(nameof(param.SectionId)));
            if (submission != null && !submission.Completed)
            {
                question = await submitAnswerCommand.GetNextUnansweredQuestion(submission.Id);
                if (question == null)
                {
                    TempData["CheckAnswersPage"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ParameterCheckAnswersPage() { SubmissionId = submission.Id, SectionId = param.SectionId, SectionName = param.SectionName });
                    return RedirectToAction("CheckAnswersPage", "CheckAnswers");
                }

                submissionId = submission.Id;
            }
        }

        var viewModel = new QuestionViewModel()
        {
            Question = question ?? await submitAnswerCommand.GetQuestionnaireQuestion(id, section, cancellationToken),
            AnswerRef = answerRef,
            Params = parameters != null ? parameters.ToString() : null,
            SubmissionId = submissionId,
        };

        return View("Question", viewModel);
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
            TempData["QuestionPage"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ParameterQuestionPage() { QuestionRef = submitAnswerDto.QuestionId, SubmissionId = submitAnswerDto.SubmissionId });
            return RedirectToAction("GetQuestionById");
        }

        int submissionId = await submitAnswerCommand.SubmitAnswer(submitAnswerDto, param.SectionId, param.SectionName);
        string? nextQuestionId = await submitAnswerCommand.GetNextQuestionId(submitAnswerDto.QuestionId, submitAnswerDto.ChosenAnswerId);

        if (string.IsNullOrEmpty(nextQuestionId) || await submitAnswerCommand.NextQuestionIsAnswered(submissionId, nextQuestionId))
        {
            TempData["CheckAnswersPage"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ParameterCheckAnswersPage() { SubmissionId = submissionId, SectionId = param.SectionId, SectionName = param.SectionName });
            return RedirectToAction("CheckAnswersPage", "CheckAnswers");
        }
        else
        {
            TempData["QuestionPage"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ParameterQuestionPage() { QuestionRef = nextQuestionId, SubmissionId = submissionId });
            return RedirectToAction("GetQuestionById");
        }
    }

    private ParameterQuestionPage _DeserialiseParameter(object? parameterObject)
    {
        if (parameterObject == null) throw new ArgumentNullException(nameof(parameterObject));

        var parameterQuestionPage = Newtonsoft.Json.JsonConvert.DeserializeObject<ParameterQuestionPage>(parameterObject as string ?? throw new ArithmeticException(nameof(parameterObject)));

        return parameterQuestionPage ?? throw new NullReferenceException(nameof(parameterQuestionPage));
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
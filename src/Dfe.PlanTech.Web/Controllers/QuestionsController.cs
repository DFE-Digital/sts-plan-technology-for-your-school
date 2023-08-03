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
        string id,
        string? section,
        int? submissionId,
        string? answerRef,
        [FromServices] SubmitAnswerCommand submitAnswerCommand,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        object? parameters;

        TempData.TryGetValue("param", out parameters);

        Question? question = null;

        if (submissionId == null)
        {
            Params? param = _ParseParameters(parameters?.ToString());
            var submission = await submitAnswerCommand.GetOngoingSubmission(param?.SectionId ?? throw new NullReferenceException(nameof(param.SectionId)));
            if (submission != null && !submission.Completed)
            {
                submissionId = submission.Id;
                question = await submitAnswerCommand.GetNextUnansweredQuestion(submission);
                if (question == null) return RedirectToAction("CheckAnswersPage", "CheckAnswers", new { submissionId = submissionId, sectionId = param?.SectionId, sectionName = param?.SectionName });
            }
        }

        if (question == null) question = await submitAnswerCommand.GetQuestionnaireQuestion(id, section, cancellationToken);

        var viewModel = new QuestionViewModel()
        {
            Question = question,
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

        if (!ModelState.IsValid) return RedirectToAction("GetQuestionById", new { id = submitAnswerDto.QuestionId, submissionId = submitAnswerDto.SubmissionId });

        Params param = new Params();
        if (!string.IsNullOrEmpty(submitAnswerDto.Params))
        {
            param = _ParseParameters(submitAnswerDto.Params) ?? null!;
            TempData["param"] = submitAnswerDto.Params;
        }

        int submissionId = await submitAnswerCommand.SubmitAnswer(submitAnswerDto, param.SectionId, param.SectionName);
        string? nextQuestionId = await submitAnswerCommand.GetNextQuestionId(submitAnswerDto.QuestionId, submitAnswerDto.ChosenAnswerId);

        if (string.IsNullOrEmpty(nextQuestionId) || await submitAnswerCommand.NextQuestionIsAnswered(submissionId, nextQuestionId)) return RedirectToAction("CheckAnswersPage", "CheckAnswers", new { submissionId = submissionId, sectionId = param.SectionId, sectionName = param.SectionName });
        else return RedirectToAction("GetQuestionById", new { id = nextQuestionId, submissionId = submissionId });
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
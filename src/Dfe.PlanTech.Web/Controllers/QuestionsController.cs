using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
[Route("/question")]
public class QuestionsController : BaseController<QuestionsController>
{
    public QuestionsController(ILogger<QuestionsController> logger, IUrlHistory history) : base(logger, history)
    {
    }

    [HttpGet("{id?}")]
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="section">Name of current section (if starting new)</param>
    /// <param name="query"></param>
    /// <exception cref="ArgumentNullException">Throws exception when Id is null or empty</exception>
    /// <returns></returns>
    public async Task<IActionResult> GetQuestionById(string id, string? section, [FromServices] GetQuestionQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

        var question = await query.GetQuestionById(id, section, cancellationToken) ?? throw new KeyNotFoundException($"Could not find question with id {id}");

        var viewModel = new QuestionViewModel()
        {
            Question = question,
            BackUrl = history.LastVisitedUrl?.ToString() ?? "self-assessment"
        };

        return View("Question", viewModel);
    }

    [HttpPost("SubmitAnswer")]
    public async Task<IActionResult> SubmitAnswer(SubmitAnswerDto submitAnswerDto)
    {
        if (submitAnswerDto == null) throw new ArgumentNullException(nameof(submitAnswerDto));

        if (!ModelState.IsValid) return RedirectToAction("GetQuestionById", new { id = submitAnswerDto.QuestionId });

        // TODO: Figure out how to get the actual AnswerText and ContentfulRef
        var recordAnswerCommand = HttpContext.RequestServices.GetRequiredService<IRecordAnswerCommand>();
        await recordAnswerCommand.RecordAnswer(new RecordAnswerDto() { AnswerText = "Answer", ContentfulRef = "ABC123" });

        if (string.IsNullOrEmpty(submitAnswerDto.NextQuestionId)) return RedirectToAction("GetByRoute", "Pages", new { route = "check-answers" });
        else return RedirectToAction("GetQuestionById", new { id = submitAnswerDto.NextQuestionId });
    }
}

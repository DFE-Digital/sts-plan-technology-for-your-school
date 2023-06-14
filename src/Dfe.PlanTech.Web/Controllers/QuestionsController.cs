using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

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
    /// <param name="query"></param>
    /// <exception cref="ArgumentNullException">Throws exception when Id is null or empty</exception>
    /// <returns></returns>
    public async Task<IActionResult> GetQuestionById(string id, string? section, CancellationToken cancellationToken, [FromServices] GetQuestionQuery query, [FromServices] ICacher cacher)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        
        if(section != null){
            cacher.Set("CurrentSection", TimeSpan.FromMinutes(10), section);
        }

        var question = await query.GetQuestionById(id, cancellationToken) ?? throw new KeyNotFoundException($"Could not find question with id {id}");
        
        var viewModel = new QuestionViewModel()
        {
            Question = question,
            BackUrl = history.LastVisitedUrl?.ToString() ?? "self-assessment"
        };

        return View("Question", viewModel);
    }

    [HttpPost("SubmitAnswer")]
    public IActionResult SubmitAnswer(SubmitAnswerDto submitAnswerDto)
    {
        if (submitAnswerDto == null) throw new ArgumentNullException(nameof(submitAnswerDto));

        if (string.IsNullOrEmpty(submitAnswerDto.NextQuestionId)) return RedirectToAction("GetByRoute", "Pages", new { route = "check-answers" });

        return RedirectToAction("GetQuestionById", new { id = submitAnswerDto.NextQuestionId });
    }
}

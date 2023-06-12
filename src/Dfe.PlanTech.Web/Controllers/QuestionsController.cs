using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("/question")]
public class QuestionsController : BaseController<QuestionsController>
{
    public QuestionsController(ICacher cacher, ILogger<QuestionsController> logger) : base(cacher, logger)
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
    public async Task<IActionResult> GetQuestionById(string id, [FromServices] GetQuestionQuery query)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

        var question = await query.GetQuestionById(id);

        if (question == null) throw new KeyNotFoundException($"Could not find question with id {id}");

        var viewModel = new QuestionViewModel()
        {
            Question = question,
            BackUrl = GetLastVisitedUrl()
        };

        return View("Question", viewModel);
    }

    [HttpPost("SubmitAnswer")]
    public async Task<IActionResult> SubmitAnswer(SubmitAnswerDto submitAnswerDto)
    {
        if (submitAnswerDto == null) throw new ArgumentNullException(nameof(submitAnswerDto));

        if (string.IsNullOrEmpty(submitAnswerDto.NextQuestionId)) return RedirectToAction("GetByRoute", "Pages", new { route = "self-assessment" });

        return RedirectToAction("GetQuestionById", new { id = submitAnswerDto.NextQuestionId });
    }
}

using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("/question")]
public class QuestionsController : Controller
{
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(ILogger<QuestionsController> logger)
    {
        _logger = logger;
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

        return View("Question", question);
    }

    [HttpPost("SubmitAnswer")]
    public async Task<IActionResult> SubmitAnswer(SubmitAnswerDto submitAnswerDto)
    {
        if (submitAnswerDto == null) throw new ArgumentNullException(nameof(submitAnswerDto));

        if (string.IsNullOrEmpty(submitAnswerDto.NextQuestionId)) return RedirectToAction("CheckYourAnswers");

        return RedirectToAction("GetQuestionById", new { id = submitAnswerDto.NextQuestionId });
    }

    [HttpGet("check-answers")]
    public async Task<IActionResult> CheckYourAnswers()
    {
        return View("CheckYourAnswers");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error!");
    }
}

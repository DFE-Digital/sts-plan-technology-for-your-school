using Dfe.PlanTech.Application.Questionnaire.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

public class QuestionsController : Controller
{
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(ILogger<QuestionsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/question/{id?}")]
    public async Task<IActionResult> GetQuestionById(string id, [FromServices] GetQuestionQuery query)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

        var question = await query.GetQuestionById(id);

        if (question == null) throw new KeyNotFoundException($"Could not find question with id {id}");

        return View("Question", question);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error!");
    }
}

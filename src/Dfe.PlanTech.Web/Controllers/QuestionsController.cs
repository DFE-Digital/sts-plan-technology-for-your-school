using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("/question")]
public class QuestionsController : BaseController<QuestionsController>
{
    private readonly ISectionCacher _sectionCacher;
    
    public QuestionsController(ISectionCacher sectionCacher, ILogger<QuestionsController> logger, IUrlHistoryCacher history) : base(logger, history)
    {
        _sectionCacher = sectionCacher;
    }

    [HttpGet("{id?}")]
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="query"></param>
    /// <exception cref="ArgumentNullException">Throws exception when Id is null or empty</exception>
    /// <returns></returns>
    public async Task<IActionResult> GetQuestionById(string id, string? section, CancellationToken cancellationToken, [FromServices] GetQuestionQuery query)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        
        if(section != null){
            _sectionCacher.SetCurrentSection(section);
        }

        var question = await query.GetQuestionById(id, cancellationToken) ?? throw new KeyNotFoundException($"Could not find question with id {id}");
        
        var viewModel = new QuestionViewModel()
        {
            Question = question,
            BackUrl = history.LastVisitedUrl?.ToString() ?? "self-assessment" //TODO: Move this link to content model
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

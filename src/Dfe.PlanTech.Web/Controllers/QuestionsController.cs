using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Models.Inputs;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
[Route("/")]
public class QuestionsController : BaseController<QuestionsController>
{
    public const string Controller = "Questions";

    private readonly QuestionsViewBuilder _questionsViewBuilder;

    public QuestionsController(
        ILogger<QuestionsController> logger,
        QuestionsViewBuilder questionsViewBuilder
    ) : base(logger)
    {
        _questionsViewBuilder = questionsViewBuilder ?? throw new ArgumentNullException(nameof(questionsViewBuilder));
    }

    [HttpGet("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> GetQuestionBySlug(string sectionSlug, string questionSlug, string? returnTo)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(questionSlug, nameof(questionSlug));

        return await _questionsViewBuilder.RouteBySlugAndQuestionAsync(this, sectionSlug, questionSlug, returnTo);
    }

    [LogInvalidModelState]
    [HttpGet("question/preview/{questionId}")]
    public async Task<IActionResult> GetQuestionPreviewById(string questionId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(questionId, nameof(questionId));

        return await _questionsViewBuilder.RouteByQuestionId(this, questionId);
    }


    [LogInvalidModelState]
    [HttpGet("{sectionSlug}/next-question")]
    public async Task<IActionResult> GetNextUnansweredQuestion(string sectionSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));

        return await _questionsViewBuilder.RouteToNextUnansweredQuestion(this, sectionSlug);
    }

    [HttpPost("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(
        string sectionSlug,
        string questionSlug,
        SubmitAnswerInputModel answerViewModel,
        string? returnTo = ""
    )
    {
        return await _questionsViewBuilder.SubmitAnswerAndRedirect(this, answerViewModel, sectionSlug, questionSlug, returnTo);
    }
}

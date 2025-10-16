using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
[Route("/")]
public class QuestionsController : BaseController<QuestionsController>
{
    public const string Controller = "Questions";

    private readonly IQuestionsViewBuilder _questionsViewBuilder;

    public QuestionsController(
        ILogger<QuestionsController> logger,
        IQuestionsViewBuilder questionsViewBuilder
    ) : base(logger)
    {
        _questionsViewBuilder = questionsViewBuilder ?? throw new ArgumentNullException(nameof(questionsViewBuilder));
    }

    [HttpGet("{categorySlug}/{sectionSlug}/self-assessment/{questionSlug}", Name = "GetQuestionBySlug")]
    public async Task<IActionResult> GetQuestionBySlug(string categorySlug, string sectionSlug, string questionSlug, string? returnTo)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug, nameof(categorySlug));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(questionSlug, nameof(questionSlug));

        return await _questionsViewBuilder.RouteBySlugAndQuestionAsync(this, categorySlug, sectionSlug, questionSlug, returnTo);
    }

    [LogInvalidModelState]
    [HttpGet("{categorySlug}/{sectionSlug}/self-assessment", Name = "GetInterstitialPage")]
    public async Task<IActionResult> GetInterstitialPage(string categorySlug, string sectionSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug, nameof(categorySlug));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));

        return await _questionsViewBuilder.RouteToInterstitialPage(this, categorySlug, sectionSlug);
    }

    [LogInvalidModelState]
    [HttpGet("question/preview/{questionId}")]
    public async Task<IActionResult> GetQuestionPreviewById(string questionId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(questionId, nameof(questionId));

        return await _questionsViewBuilder.RouteByQuestionId(this, questionId);
    }


    [LogInvalidModelState]
    [HttpGet("{categorySlug}/{sectionSlug}/self-assessment/next-question")]
    public async Task<IActionResult> GetNextUnansweredQuestion(string categorySlug, string sectionSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug, nameof(sectionSlug));

        return await _questionsViewBuilder.RouteToNextUnansweredQuestion(this, categorySlug, sectionSlug);
    }

    [LogInvalidModelState]
    [HttpPost("{categorySlug}/{sectionSlug}/self-assessment/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(
        string categorySlug,
        string sectionSlug,
        string questionSlug,
        SubmitAnswerInputViewModel answerViewModel,
        string? returnTo = ""
    )
    {
        return await _questionsViewBuilder.SubmitAnswerAndRedirect(this, answerViewModel, categorySlug, sectionSlug, questionSlug, returnTo);
    }
}

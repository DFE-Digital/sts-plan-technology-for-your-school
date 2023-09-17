using Dfe.PlanTech.Application.Questionnaire.Interfaces;
using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Application.Submissions.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class QuestionsController : BaseController<QuestionsController>
{
    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IGetLatestResponsesQuery _getResponseQuery;
    private readonly IUser _user;

    public QuestionsController(ILogger<QuestionsController> logger,
                                IGetSectionQuery getSectionQuery,
                                IGetLatestResponsesQuery getResponseQuery,
                                IUser user) : base(logger)
    {
        _getResponseQuery = getResponseQuery;
        _getSectionQuery = getSectionQuery;
        _user = user;
    }

    [HttpGet("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> GetQuestionBySlug(string sectionSlug,
                                                        string questionSlug,
                                                        CancellationToken cancellationToken = default)
    {
        var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ?? 
                        throw new KeyNotFoundException($"Could not find section with slug {sectionSlug}");

        var question = section.Questions.FirstOrDefault(question => question.Slug == questionSlug) ??
                            throw new KeyNotFoundException($"Could not find question slug {questionSlug} under section {sectionSlug}");

        int establishmentId = await _user.GetEstablishmentId();

        var latestResponseForQuestion = await _getResponseQuery.GetLatestResponseForQuestion(establishmentId,
                                                                                section.Sys.Id,
                                                                                question.Sys.Id,
                                                                                cancellationToken);

        var viewModel = new QuestionViewModel()
        {
            Question = question,
            AnswerRef = latestResponseForQuestion?.AnswerRef,
            ErrorMessage = null,
            SectionName = section.Name,
            SectionSlug = sectionSlug,
            SectionId = section.Sys.Id
        };

        return View("Question", viewModel);
    }

    [HttpGet("{sectionSlug}/next-question")]
    public async Task<IActionResult> GetNextUnansweredQuestion(string sectionSlug,
                                                                [FromServices] IGetNextUnansweredQuestionQuery getQuestionQuery,
                                                                CancellationToken cancellationToken = default)
    {
        var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ?? throw new KeyNotFoundException($"Could not find section with slug {sectionSlug}");

        int establishmentId = await _user.GetEstablishmentId();

        var nextQuestion = await getQuestionQuery.GetNextUnansweredQuestion(establishmentId, section, cancellationToken);

        if (nextQuestion == null) return RedirectToAction("CheckAnswersPage", "CheckAnswers", new { sectionSlug });

        return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug = nextQuestion.Slug });
    }

    [HttpPost("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(string sectionSlug, string questionSlug, SubmitAnswerDto submitAnswerDto, [FromServices] ISubmitAnswerCommand submitAnswerCommand, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug });

        await submitAnswerCommand.SubmitAnswer(submitAnswerDto, cancellationToken);

        return RedirectToAction(nameof(GetNextUnansweredQuestion), new { sectionSlug });
    }
}
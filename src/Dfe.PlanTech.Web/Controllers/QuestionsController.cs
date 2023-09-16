using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Constants;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class QuestionsController : BaseController<QuestionsController>
{
    private readonly GetQuestionQuery _getQuestionQuery;
    public QuestionsController(ILogger<QuestionsController> logger, GetQuestionQuery getQuestionQuery) : base(logger)
    {
        _getQuestionQuery = getQuestionQuery;
    }

    [HttpGet("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> GetQuestionBySlug(string sectionSlug,
                                                        string questionSlug,
                                                        [FromServices] GetSectionQuery getSectionQuery,
                                                        [FromServices] IGetLatestResponseListForSubmissionQuery getResponseQuery,
                                                        [FromServices] IUser user,
                                                        CancellationToken cancellationToken = default)
    {
        var section = await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                        throw new KeyNotFoundException($"Could not find section for slug {sectionSlug}");

        var question = section.Questions.FirstOrDefault(question => question.Slug == questionSlug) ?? throw new Exception("No question");

        int establishmentId = await user.GetEstablishmentId();

        return await ShowQuestionPage(establishmentId, sectionSlug, section, question, getResponseQuery);
    }

    [HttpGet("{sectionSlug}/next-question")]
    public async Task<IActionResult> GetNextUnansweredQuestion(string sectionSlug,
                                                                [FromServices] GetSectionQuery getSectionQuery,
                                                                [FromServices] GetQuestionQuery getQuestionQuery,
                                                                [FromServices] IGetLatestResponseListForSubmissionQuery getResponseQuery,
                                                                [FromServices] IUser user,
                                                                CancellationToken cancellationToken = default)
    {
        var section = await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                throw new KeyNotFoundException($"Could not find section for slug {sectionSlug}");

        int establishmentId = await user.GetEstablishmentId();

        var nextQuestion = await getQuestionQuery.GetNextUnansweredQuestion(establishmentId, section, cancellationToken);

        if (nextQuestion == null)
        {
            return RedirectToAction("CheckAnswersPage", "CheckAnswers", new { sectionSlug });
        }

        return await ShowQuestionPage(establishmentId, sectionSlug, section, nextQuestion, getResponseQuery);
    }

    [HttpPost("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(string sectionSlug, string questionSlug, SubmitAnswerDto submitAnswerDto, [FromServices] ISubmitAnswerCommand submitAnswerCommand)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug });

        var result = await submitAnswerCommand.SubmitAnswer(submitAnswerDto, submitAnswerDto.SectionId, sectionName: sectionSlug);

        if (submitAnswerDto.ChosenAnswer?.NextQuestion == null)
        {
            return RedirectToAction("CheckAnswersPage", "CheckAnswers", new { sectionSlug });
        }

        return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug = submitAnswerDto.ChosenAnswer.NextQuestion.Value.Slug });
    }

    private async Task<IActionResult> ShowQuestionPage(int establishmentId, string sectionSlug, Section section, Question question, IGetLatestResponseListForSubmissionQuery getResponseQuery)
    {
        var latestResponseForQuestion = await getResponseQuery.GetLatestResponseForQuestion(establishmentId,
                                                                                section.Sys.Id,
                                                                                question.Sys.Id);

        var viewModel = new QuestionViewModel()
        {
            Question = question,
            AnswerRef = latestResponseForQuestion?.AnswerRef,
            ErrorMessage = null,
            SectionSlug = sectionSlug,
            SectionId = section.Sys.Id
        };

        return View("Question", viewModel);
    }
}
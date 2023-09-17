using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Application.Submissions.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
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
                                                        [FromServices] IGetLatestResponsesQuery getResponseQuery,
                                                        [FromServices] IUser user,
                                                        CancellationToken cancellationToken = default)
    {
        var section = await getSectionQuery.GetSectionBySlug(sectionSlug) ?? throw new KeyNotFoundException($"Could not find section with slug {sectionSlug}");

        var question = section.Questions.FirstOrDefault(question => question.Slug == questionSlug) ?? throw new Exception("No question");

        int establishmentId = await user.GetEstablishmentId();

        return await ShowQuestionPage(establishmentId, sectionSlug, section, question, getResponseQuery, cancellationToken);
    }

    [HttpGet("{sectionSlug}/next-question")]
    public async Task<IActionResult> GetNextUnansweredQuestion(string sectionSlug,
                                                                [FromServices] GetSectionQuery getSectionQuery,
                                                                [FromServices] GetQuestionQuery getQuestionQuery,
                                                                [FromServices] IGetLatestResponsesQuery getResponseQuery,
                                                                [FromServices] IUser user,
                                                                CancellationToken cancellationToken = default)
    {
        var section = await getSectionQuery.GetSectionBySlug(sectionSlug) ?? throw new KeyNotFoundException($"Could not find section with slug {sectionSlug}");

        int establishmentId = await user.GetEstablishmentId();

        var nextQuestion = await getQuestionQuery.GetNextUnansweredQuestion(establishmentId, section, cancellationToken);

        if (nextQuestion == null) return RedirectToAction("CheckAnswersPage", "CheckAnswers", new { sectionSlug });

        return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug = nextQuestion.Slug });
    }

    [HttpPost("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(string sectionSlug, string questionSlug, SubmitAnswerDto submitAnswerDto, [FromServices] IUser user, [FromServices] ISubmitAnswerCommand submitAnswerCommand, [FromServices] IGetLatestResponsesQuery getResponseQuery)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug });

        //TODO: Move logic to submit answer
        var result = await submitAnswerCommand.SubmitAnswer(submitAnswerDto, submitAnswerDto.SectionId, sectionName: sectionSlug);

        bool navigateToCheckAnswersPage = await GetQuestionCompletionStatus(submitAnswerDto, user, getResponseQuery);

        if (navigateToCheckAnswersPage) return RedirectToAction("CheckAnswersPage", "CheckAnswers", new { sectionSlug });

        return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug = submitAnswerDto.ChosenAnswer!.NextQuestion!.Value.Slug });
    }

    private static async Task<bool> GetQuestionCompletionStatus(SubmitAnswerDto submitAnswerDto, IUser user, IGetLatestResponsesQuery getResponseQuery)
    {
        //IF there's no next question for the answer, then section should be complete
        if (submitAnswerDto.ChosenAnswer?.NextQuestion == null) return true;

        //Otherwise, do we have a response for next question?
        //If so, we were changing our last answer, and the next question in the sequence is the same- so just navigate to check answers page
        //Otherwise, we should navigate to next question
        int establishmentId = await user.GetEstablishmentId();
        var latestResponseForQuestion = await getResponseQuery.GetLatestResponseForQuestion(establishmentId,
                                                                                            sectionId: submitAnswerDto.SectionId,
                                                                                            questionId: submitAnswerDto.ChosenAnswer!.NextQuestion!.Value.Id);

        return latestResponseForQuestion != null;
    }

    private async Task<IActionResult> ShowQuestionPage(int establishmentId, string sectionSlug, Section section, Question question, IGetLatestResponsesQuery getResponseQuery, CancellationToken cancellationToken = default)
    {
        var latestResponseForQuestion = await getResponseQuery.GetLatestResponseForQuestion(establishmentId,
                                                                                section.Sys.Id,
                                                                                question.Sys.Id,
                                                                                cancellationToken);

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
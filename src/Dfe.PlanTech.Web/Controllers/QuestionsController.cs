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
                                                        [FromServices] IGetLatestResponseListForSubmissionQuery _getResponseQuery,
                                                        [FromServices] IUser user,
                                                        CancellationToken cancellationToken = default)
    {
        //TODO: Move all logic below elsehwere
        //New class - GetNextQuestionForEstablishment
        var section = await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                        throw new KeyNotFoundException($"Could not find section for slug {sectionSlug}");

        var question = section.Questions.FirstOrDefault(question => question.Slug == questionSlug);

        //todo: address logic
        //check question answered for incomplete section - if so use answer
        var latestQuestionWithAnswer = await _getResponseQuery.GetLatestResponse(await user.GetEstablishmentId(),
                                                                                section.Sys.Id);

        if (latestQuestionWithAnswer != null)
        {
            var answeredQuestion = section.Questions.Select(q =>
                        q.Answers.FirstOrDefault(answer => answer.Sys.Id == latestQuestionWithAnswer.AnswerRef && q.Sys.Id == latestQuestionWithAnswer.QuestionRef)).FirstOrDefault();

            var nextQuestion = answeredQuestion?.NextQuestion != null ? section.Questions.FirstOrDefault(q => q.Sys.Id == answeredQuestion.NextQuestion.Sys.Id) : null;

            if (nextQuestion != null)
            {
                var model = new QuestionViewModel()
                {
                    Question = nextQuestion,
                    AnswerRef = null,
                    Params = null,
                    SubmissionId = 1,
                    QuestionErrorMessage = null,
                    SectionSlug = sectionSlug,
                    SectionId = section.Sys.Id
                };

                return View("Question", model);
            }
            else
            {
                //TODO: REDIRECT TO CHECK ANSWERS
                return RedirectToAction("CheckAnswersPage", "CheckAnswers", new { sectionSlug });
            }
        }

        if (question == null) throw new Exception("No question");

        var viewModel = new QuestionViewModel()
        {
            Question = question,
            AnswerRef = null,
            Params = null,
            SubmissionId = 1,
            QuestionErrorMessage = null,
            SectionSlug = sectionSlug,
            SectionId = section.Sys.Id
        };

        return View("Question", viewModel);
    }

    [HttpPost("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(string sectionSlug, string questionSlug, SubmitAnswerDto submitAnswerDto, [FromServices] ISubmitAnswerCommand submitAnswerCommand)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug });

        var result = await submitAnswerCommand.SubmitAnswer(submitAnswerDto, submitAnswerDto.SectionId, sectionName: sectionSlug);

        if (submitAnswerDto.ChosenAnswer?.NextQuestion == null)
        {
            //TODO: Redirect to check answers page
            return Redirect("/self-assessment");
        }

        return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug = submitAnswerDto.ChosenAnswer.NextQuestion.Value.Slug });
    }
}
using System.Diagnostics;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    private readonly GetQuestionQuery _getQuestionnaireQuery;
    private readonly ICalculateMaturityCommand _calculateMaturityCommand;
    private readonly IGetResponseQuery _getResponseQuery;
    private readonly IGetQuestionQuery _getQuestionQuery;
    private readonly IGetAnswerQuery _getAnswerQuery;
    private readonly GetPageQuery _getPageQuery;

    public CheckAnswersController(ILogger<CheckAnswersController> logger, IUrlHistory history,
                                  [FromServices] GetQuestionQuery getQuestionnaireQuery,
                                  [FromServices] ICalculateMaturityCommand calculateMaturityCommand,
                                  [FromServices] IGetResponseQuery getResponseQuery,
                                  [FromServices] IGetQuestionQuery getQuestionQuery,
                                  [FromServices] IGetAnswerQuery getAnswerQuery,
                                  [FromServices] GetPageQuery getPageQuery) : base(logger, history)
    {
        _getQuestionnaireQuery = getQuestionnaireQuery;
        _calculateMaturityCommand = calculateMaturityCommand;
        _getResponseQuery = getResponseQuery;
        _getQuestionQuery = getQuestionQuery;
        _getAnswerQuery = getAnswerQuery;
        _getPageQuery = getPageQuery;
    }

    private async Task<QuestionWithAnswer> _CreateQuestionWithAnswer(string questionRef, string questionText, int answerId)
    {
        var answer = await _GetResponseAnswer(answerId);
        string answerContentfulRef = answer?.ContentfulRef ?? throw new NullReferenceException(nameof(answer.ContentfulRef));
        string answerText = answer?.AnswerText ?? throw new NullReferenceException(nameof(answerText));

        return new QuestionWithAnswer()
        {
            QuestionRef = questionRef,
            QuestionText = questionText,
            AnswerRef = answerContentfulRef,
            AnswerText = answerText
        };
    }

    private async Task<CheckAnswerDto> _RemoveDetachedQuestions(CheckAnswerDto checkAnswerDto)
    {
        int questionAnswerListCount = checkAnswerDto.QuestionAnswerList.Count();
        if (questionAnswerListCount <= 1) return checkAnswerDto;

        Dictionary<string, bool> isDetachedMap = new Dictionary<string, bool>();

        for (int i = 0; i < questionAnswerListCount; i++) isDetachedMap.Add(checkAnswerDto.QuestionAnswerList[i].QuestionRef, i == 0 ? false : true);

        foreach (QuestionWithAnswer questionWithAnswer in checkAnswerDto.QuestionAnswerList)
        {
            Domain.Questionnaire.Models.Answer answer = await _GetAnswer(questionWithAnswer.QuestionRef, questionWithAnswer.AnswerRef) ?? throw new NullReferenceException(nameof(answer));
            string? nextQuestionId = answer.NextQuestion?.Sys.Id;
            if (nextQuestionId == null) continue;
            if (isDetachedMap.ContainsKey(nextQuestionId)) isDetachedMap[nextQuestionId] = false;
        }

        checkAnswerDto.QuestionAnswerList.RemoveAll(questionWithAnswer => isDetachedMap[questionWithAnswer.QuestionRef]);

        return checkAnswerDto;
    }

    private async Task<CheckAnswerDto> _GetCheckAnswerDto(Response[] responseList)
    {
        CheckAnswerDto checkAnswerDto = new CheckAnswerDto();

        Dictionary<string, int> indexMap = new Dictionary<string, int>();
        Dictionary<string, DateTime> dateTimeMap = new Dictionary<string, DateTime>();

        int index = 0;

        foreach (Response response in responseList)
        {
            var question = await _GetResponseQuestion(response.QuestionId);
            string questionContentfulRef = question?.ContentfulRef ?? throw new NullReferenceException(nameof(question.ContentfulRef));
            string questionText = question?.QuestionText ?? throw new NullReferenceException(nameof(questionText));

            if (dateTimeMap.ContainsKey(questionContentfulRef))
            {
                if (DateTime.Compare(question.DateCreated, dateTimeMap[questionContentfulRef]) > 0)
                {
                    checkAnswerDto.QuestionAnswerList[indexMap[questionContentfulRef]] = await _CreateQuestionWithAnswer(questionContentfulRef, questionText, response.AnswerId);
                }
            }
            else
            {
                checkAnswerDto.QuestionAnswerList.Add(await _CreateQuestionWithAnswer(questionContentfulRef, questionText, response.AnswerId));
                dateTimeMap.Add(questionContentfulRef, question.DateCreated);
                indexMap.Add(questionContentfulRef, index++);
            }
        }

        return await _RemoveDetachedQuestions(checkAnswerDto);
    }

    [HttpGet]
    [Route("check-answers")]
    public async Task<IActionResult> CheckAnswersPage(int submissionId, string sectionName)
    {
        Response[]? responseList = await _GetResponseList(submissionId);

        Page checkAnswerPageContent = await _GetCheckAnswerContent();

        CheckAnswersViewModel checkAnswersViewModel = new CheckAnswersViewModel()
        {
            BackUrl = history.LastVisitedUrl?.ToString() ?? "self-assessment",
            Title = checkAnswerPageContent.Title ?? throw new NullReferenceException(nameof(checkAnswerPageContent.Title)),
            SectionName = sectionName,
            CheckAnswerDto = await _GetCheckAnswerDto(responseList ?? throw new NullReferenceException(nameof(responseList))),
            Content = checkAnswerPageContent.Content,
            SubmissionId = submissionId
        };

        return View("CheckAnswers", checkAnswersViewModel);
    }

    [HttpGet]
    [Route("change-answer")]
    public IActionResult ChangeAnswer(string questionRef, string answerRef, int submissionId)
    {
        return RedirectToAction("GetQuestionById", "Questions", new { id = questionRef, answerRef = answerRef, submissionId = submissionId });
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId)
    {
        var calculateMaturity = await _calculateMaturityCommand.CalculateMaturityAsync(submissionId);

        if (calculateMaturity > 1)
        {
            return RedirectToAction("GetByRoute", "Pages", new { route = "self-assessment" });
        }

        // TODO Show error message.
        return null;
    }

    private async Task<Response[]?> _GetResponseList(int submissionId)
    {
        return await _getResponseQuery.GetResponseListBy(submissionId);
    }

    private async Task<Domain.Questions.Models.Question?> _GetResponseQuestion(int questionId)
    {
        return await _getQuestionQuery.GetQuestionBy(questionId);
    }

    private async Task<Domain.Answers.Models.Answer?> _GetResponseAnswer(int answerId)
    {
        return await _getAnswerQuery.GetAnswerBy(answerId);
    }

    private async Task<Domain.Questionnaire.Models.Answer?> _GetAnswer(string questionRef, string answerRef)
    {
        if (string.IsNullOrEmpty(questionRef)) throw new ArgumentNullException(nameof(questionRef));
        return (await _getQuestionnaireQuery.GetQuestionById(questionRef, null, CancellationToken.None) ?? throw new KeyNotFoundException($"Could not find answer with id {answerRef}")).Answers.First(answer => answer.Sys.Id.Equals(answerRef));
    }

    private async Task<Page> _GetCheckAnswerContent()
    {
        return await _getPageQuery.GetPageBySlug("check-answers", CancellationToken.None);
    }
}
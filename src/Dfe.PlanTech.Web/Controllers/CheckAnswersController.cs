using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
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
[Route("/check-answers")]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    private readonly ICalculateMaturityCommand _calculateMaturityCommand;
    private readonly IGetResponseQuery _getResponseQuery;
    private readonly IGetQuestionQuery _getQuestionQuery;
    private readonly IGetAnswerQuery _getAnswerQuery;
    private readonly GetPageQuery _getPageQuery;

    public CheckAnswersController(ILogger<CheckAnswersController> logger, IUrlHistory history,
                                  [FromServices] ICalculateMaturityCommand calculateMaturityCommand,
                                  [FromServices] IGetResponseQuery getResponseQuery,
                                  [FromServices] IGetQuestionQuery getQuestionQuery,
                                  [FromServices] IGetAnswerQuery getAnswerQuery,
                                  [FromServices] GetPageQuery getPageQuery) : base(logger, history)
    {
        _calculateMaturityCommand = calculateMaturityCommand;
        _getResponseQuery = getResponseQuery;
        _getQuestionQuery = getQuestionQuery;
        _getAnswerQuery = getAnswerQuery;
        _getPageQuery = getPageQuery;
    }

    private async Task<Response[]?> _GetResponseList(int submissionId)
    {
        return await _getResponseQuery.GetResponseListBy(submissionId);
    }

    private async Task<string?> _GetResponseQuestionText(int questionId)
    {
        return (await _getQuestionQuery.GetQuestionBy(questionId))?.QuestionText;
    }

    private async Task<string?> _GetResponseAnswerText(int answerId)
    {
        return (await _getAnswerQuery.GetAnswerBy(answerId))?.AnswerText;
    }

    private async Task<CheckAnswerDto> _GetCheckAnswerDto(Response[] responseList)
    {
        CheckAnswerDto checkAnswerDto = new CheckAnswerDto() { QuestionAnswerList = new QuestionWithAnswer[responseList.Length] };

        for (int i = 0; i < checkAnswerDto.QuestionAnswerList.Length; i++)
        {
            string questionText = await _GetResponseQuestionText(responseList[i].QuestionId) ?? throw new NullReferenceException(nameof(questionText));
            string answerText = await _GetResponseAnswerText(responseList[i].AnswerId) ?? throw new NullReferenceException(nameof(answerText));

            checkAnswerDto.QuestionAnswerList[i] = new QuestionWithAnswer()
            {
                QuestionText = questionText,
                AnswerText = answerText
            };
        }

        return checkAnswerDto;
    }

    private async Task<Page> _GetCheckAnswerContent()
    {
        return await _getPageQuery.GetPageBySlug("check-answers", CancellationToken.None);
    }

    [HttpGet]
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

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId)
    {
        var calculateMaturity = await _calculateMaturityCommand.CalculateMaturityAsync(submissionId);

        if (calculateMaturity > 1)
        {
            return RedirectToAction("Pages", "Index");
        }

        // TODO Show error message.
        return null;
    }
}
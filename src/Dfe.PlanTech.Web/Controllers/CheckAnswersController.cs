using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Responses.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    private readonly ProcessCheckAnswerDtoCommand _processCheckAnswerDtoCommand;
    private readonly ICalculateMaturityCommand _calculateMaturityCommand;
    private readonly IGetResponseQuery _getResponseQuery;
    private readonly GetPageQuery _getPageQuery;

    public CheckAnswersController(
        ILogger<CheckAnswersController> logger,
        IUrlHistory history,
        [FromServices] GetQuestionQuery getQuestionnaireQuery,
        [FromServices] ICalculateMaturityCommand calculateMaturityCommand,
        [FromServices] IGetResponseQuery getResponseQuery,
        [FromServices] IGetQuestionQuery getQuestionQuery,
        [FromServices] IGetAnswerQuery getAnswerQuery,
        [FromServices] GetPageQuery getPageQuery) : base(logger, history)
    {
        _processCheckAnswerDtoCommand = new ProcessCheckAnswerDtoCommand(getQuestionQuery, getAnswerQuery, getQuestionnaireQuery);
        _calculateMaturityCommand = calculateMaturityCommand;
        _getResponseQuery = getResponseQuery;
        _getPageQuery = getPageQuery;
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
            CheckAnswerDto = await _processCheckAnswerDtoCommand.ProcessCheckAnswerDto(responseList ?? throw new NullReferenceException(nameof(responseList))),
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
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId, string sectionName)
    {
        await _calculateMaturityCommand.CalculateMaturityAsync(submissionId);

        TempData["SectionName"] = sectionName;
        return RedirectToAction("GetByRoute", "Pages", new { route = "self-assessment" });
            
    }

    private async Task<Response[]?> _GetResponseList(int submissionId)
    {
        return await _getResponseQuery.GetResponseListBy(submissionId);
    }

    private async Task<Page> _GetCheckAnswerContent()
    {
        return await _getPageQuery.GetPageBySlug("check-answers", CancellationToken.None);
    }
}
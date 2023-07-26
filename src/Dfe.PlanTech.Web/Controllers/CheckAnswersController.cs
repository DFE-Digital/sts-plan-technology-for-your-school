using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    public CheckAnswersController(ILogger<CheckAnswersController> logger, IUrlHistory history) : base(logger, history) { }

    [HttpGet]
    [Route("check-answers")]
    public async Task<IActionResult> CheckAnswersPage(
        int submissionId,
        string sectionName,
        [FromServices] ProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand,
        [FromServices] GetPageQuery getPageQuery)
    {
        Page checkAnswerPageContent = await getPageQuery.GetPageBySlug("check-answers", CancellationToken.None);

        CheckAnswersViewModel checkAnswersViewModel = new CheckAnswersViewModel()
        {
            BackUrl = history.LastVisitedUrl?.ToString() ?? "self-assessment",
            Title = checkAnswerPageContent.Title ?? throw new NullReferenceException(nameof(checkAnswerPageContent.Title)),
            SectionName = sectionName,
            CheckAnswerDto = await processCheckAnswerDtoCommand.ProcessCheckAnswerDto(submissionId),
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
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId, string sectionName, [FromServices] ProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand)
    {
        await processCheckAnswerDtoCommand.CalculateMaturityAsync(submissionId);

        TempData["SectionName"] = sectionName;
        return RedirectToAction("GetByRoute", "Pages", new { route = "self-assessment" });

    }
}
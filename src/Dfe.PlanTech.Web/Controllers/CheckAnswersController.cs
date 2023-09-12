using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Constants;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.SignIn.Extensions;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    public CheckAnswersController(ILogger<CheckAnswersController> logger) : base(logger) { }

    [HttpGet]
    [Route("check-answers")]
    public async Task<IActionResult> CheckAnswersPage([FromServices] ProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand, [FromServices] GetPageQuery getPageQuery)
    {
        var parameterCheckAnswersPage = DeserialiseParameter<TempDataCheckAnswers>(TempData[TempDataConstants.CheckAnswers]);

        Page checkAnswerPageContent = await getPageQuery.GetPageBySlug("check-answers", CancellationToken.None);

        CheckAnswersViewModel checkAnswersViewModel = new CheckAnswersViewModel()
        {
            Title = checkAnswerPageContent.Title ?? throw new NullReferenceException(nameof(checkAnswerPageContent.Title)),
            SectionName = parameterCheckAnswersPage.SectionName,
            CheckAnswerDto = await processCheckAnswerDtoCommand.ProcessCheckAnswerDto(parameterCheckAnswersPage.SubmissionId, parameterCheckAnswersPage.SectionId),
            Content = checkAnswerPageContent.Content,
            SubmissionId = parameterCheckAnswersPage.SubmissionId
        };

        return View("CheckAnswers", checkAnswersViewModel);
    }

    [HttpGet]
    [Route("{sectionName}/check-answers")]
    public async Task<IActionResult> CheckAnswersPage(string sectionName, [FromServices] ProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand, [FromServices] GetPageQuery getPageQuery)
    {
        Page checkAnswerPageContent = await getPageQuery.GetPageBySlug("check-answers", CancellationToken.None);

        var organisation = HttpContext.User.Claims.GetOrganisation();

        var latestSubmissionList = await processCheckAnswerDtoCommand.GetLatestSubmissionAnswers(sectionName, organisation!.Reference);

        CheckAnswersViewModel checkAnswersViewModel = new()
        {
            Title = checkAnswerPageContent.Title ?? throw new NullReferenceException(nameof(checkAnswerPageContent.Title)),
            SectionName = sectionName,
            CheckAnswerDto =latestSubmissionList,
            Content = checkAnswerPageContent.Content,
            SubmissionId = latestSubmissionList.QuestionAnswerList.Select(questionWithAnswer => questionWithAnswer.SubmissionId).FirstOrDefault()
        };

        return View("CheckAnswers", checkAnswersViewModel);
    }


    [HttpGet]
    [Route("change-answer")]
    public IActionResult ChangeAnswer(string questionRef, string answerRef, int submissionId)
    {
        TempData[TempDataConstants.Questions] = SerialiseParameter(new TempDataQuestions() { QuestionRef = questionRef, AnswerRef = answerRef, SubmissionId = submissionId });
        return RedirectToAction("GetQuestionById", "Questions");
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId, string sectionName, [FromServices] ProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand)
    {
        await processCheckAnswerDtoCommand.CalculateMaturityAsync(submissionId);

        TempData["SectionName"] = sectionName;
        return RedirectToAction("GetByRoute", "Pages", new { route = "self-assessment" });

    }
}
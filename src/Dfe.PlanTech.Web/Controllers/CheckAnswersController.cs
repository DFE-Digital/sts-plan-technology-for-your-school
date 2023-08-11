using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
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
        ParameterCheckAnswersPage parameterCheckAnswersPage = _DeserialiseParameter(TempData["CheckAnswersPage"]);

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
    [Route("change-answer")]
    public IActionResult ChangeAnswer(string questionRef, string answerRef, int submissionId)
    {
        TempData["QuestionPage"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ParameterQuestionPage() { QuestionRef = questionRef, AnswerRef = answerRef, SubmissionId = submissionId });
        return RedirectToAction("GetQuestionById", "Questions");
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId, string sectionName, [FromServices] ProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand)
    {
        await processCheckAnswerDtoCommand.CalculateMaturityAsync(submissionId);

        TempData["SectionName"] = sectionName;
        return RedirectToAction("GetByRoute", "Pages", new { route = "self-assessment" });

    }

    private ParameterCheckAnswersPage _DeserialiseParameter(object? parameterObject)
    {
        if (parameterObject == null) throw new ArgumentNullException(nameof(parameterObject));
        if (parameterObject is not string) throw new ArithmeticException(nameof(parameterObject));

        var parameterCheckAnswersPage = Newtonsoft.Json.JsonConvert.DeserializeObject<ParameterCheckAnswersPage>((string)parameterObject);

        return parameterCheckAnswersPage ?? throw new NullReferenceException(nameof(parameterCheckAnswersPage));
    }
}
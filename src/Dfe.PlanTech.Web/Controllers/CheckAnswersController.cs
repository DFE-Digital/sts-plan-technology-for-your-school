using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Constants;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    private readonly GetQuestionQuery _getQuestionnaireQuery;

    public CheckAnswersController(ILogger<CheckAnswersController> logger, GetQuestionQuery getQuestionQuery) : base(logger) 
    {
        this._getQuestionnaireQuery = getQuestionQuery;  
    }

    [HttpGet]
    [Route("{SectionSlug}/check-answers")]
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
            SectionSlug = parameterCheckAnswersPage.SectionSlug,
            SubmissionId = parameterCheckAnswersPage.SubmissionId,
            Slug = checkAnswerPageContent.Slug
        };

        return View("CheckAnswers", checkAnswersViewModel);
    }

    [HttpGet]
    [Route("change-answer", Name = "ChangeAnswerRouteLink")]
    public async Task<IActionResult> ChangeAnswer(string questionRef, string answerRef, int submissionId, string slug)
    {
        TempData[TempDataConstants.Questions] = SerialiseParameter(new TempDataQuestions() { QuestionRef = questionRef, AnswerRef = answerRef, SubmissionId = submissionId });
        var paramData = TempData.Peek("param");
        var param = ParamParser._ParseParameters(paramData?.ToString());
        var question = await _getQuestionnaireQuery.GetQuestionById(questionRef);
        // return RedirectPermanent($"~/{param?.SectionSlug}/{question?.Slug}/{questionRef}");
        return RedirectToRoute("SectionQuestionAnswer", new { sectionSlug = param?.SectionSlug, question = question?.Slug });
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId, string sectionName, [FromServices] ProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand)
    {
        await processCheckAnswerDtoCommand.CalculateMaturityAsync(submissionId);

        TempData["SectionName"] = sectionName;
        return RedirectToAction("GetByRoute", "Pages", new { route = "self-assessment" });

    }
}
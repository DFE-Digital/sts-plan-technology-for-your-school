using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
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

    [HttpGet("{SectionSlug}/{question}", Name = "SectionQuestionAnswer")]
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="section">Name of current section (if starting new)</param>
    /// <returns></returns>
    public async Task<IActionResult> GetQuestionById(string? section, [FromServices] ISubmitAnswerCommand submitAnswerCommand, CancellationToken cancellationToken)
    {
        var parameterQuestionPage = TempData[TempDataConstants.Questions] != null ? DeserialiseParameter<TempDataQuestions>(TempData[TempDataConstants.Questions]) : new TempDataQuestions();
        string id = string.Empty;

        if (!string.IsNullOrEmpty(TempData.Peek("questionId") as string))
        {
            id = TempData["questionId"] as string;
        }
        else
        {
            id = parameterQuestionPage.QuestionRef;
        }

        TempData.TryGetValue("param", out object? parameters);
        Params? param = ParamParser._ParseParameters(parameters?.ToString());

        var questionWithSubmission = await submitAnswerCommand.GetQuestionWithSubmission(parameterQuestionPage.SubmissionId, id, param?.SectionId ?? throw new NullReferenceException(nameof(param)), section, cancellationToken);

        if (questionWithSubmission.Question == null)
        {
            TempData[TempDataConstants.CheckAnswers] = SerialiseParameter(new TempDataCheckAnswers() { SubmissionId = questionWithSubmission.Submission?.Id ?? throw new NullReferenceException(nameof(questionWithSubmission.Submission)), SectionId = param.SectionId, SectionName = param.SectionName, SectionSlug = param.SectionSlug });
            return RedirectToRoute("CheckAnswersRoute", new { sectionSlug = param.SectionSlug });
        }
        else
                                                {
            var viewModel = new QuestionViewModel()
            {
                Question = questionWithSubmission.Question,
                AnswerRef = parameterQuestionPage.AnswerRef,
                Params = parameters?.ToString(),
                SubmissionId = questionWithSubmission.Submission == null ? parameterQuestionPage.SubmissionId : questionWithSubmission.Submission.Id,
                QuestionErrorMessage = parameterQuestionPage.NoSelectedAnswerErrorMessage
            };

            return View("Question", viewModel);
        }
    }

    [HttpPost("/question/SubmitAnswer")]
    public async Task<IActionResult> SubmitAnswer(SubmitAnswerDto submitAnswerDto, [FromServices] ISubmitAnswerCommand submitAnswerCommand)
    {
        if (submitAnswerDto == null) throw new ArgumentNullException(nameof(submitAnswerDto));

        Params param = new Params();
        if (!string.IsNullOrEmpty(submitAnswerDto.Params))
        {
            param = ParamParser._ParseParameters(submitAnswerDto.Params) ?? null!;
            TempData["param"] = submitAnswerDto.Params;
        }

        if (!ModelState.IsValid)
        {
            TempData[TempDataConstants.Questions] = SerialiseParameter(new TempDataQuestions()
            {
                QuestionRef = submitAnswerDto.QuestionId,
                SubmissionId = submitAnswerDto.SubmissionId,
                NoSelectedAnswerErrorMessage = "You must select an answer to continue"
            });
            return RedirectToAction("GetQuestionById");
        }

        int submissionId;
        
        try
        {
            submissionId = await submitAnswerCommand.SubmitAnswer(submitAnswerDto, param.SectionId, param.SectionName);
        }
        catch (Exception e)
        {
            logger.LogError("An error has occurred while submitting an answer with the following message: {} ", e.Message);
            
            TempData[TempDataConstants.Questions] = SerialiseParameter(new TempDataQuestions()
            {
                QuestionRef = submitAnswerDto.QuestionId,
                SubmissionId = submitAnswerDto.SubmissionId,
                NoSelectedAnswerErrorMessage = "Save failed. Please try again later."
            });
            return RedirectToAction("GetQuestionById");
        }

        string? nextQuestionId;
        
        try
        {
            nextQuestionId = await submitAnswerCommand.GetNextQuestionId(submitAnswerDto.QuestionId, submitAnswerDto.ChosenAnswerId);
        }
        catch (Exception e)
        {
            logger.LogError( "An error has occurred while retrieving the next question with the following message: {} ", e.Message);
            return Redirect(UrlConstants.ServiceUnavailable);
        }
        

        if (string.IsNullOrEmpty(nextQuestionId) || await submitAnswerCommand.NextQuestionIsAnswered(submissionId, nextQuestionId))
        {
            TempData[TempDataConstants.CheckAnswers] = SerialiseParameter(new TempDataCheckAnswers() { SubmissionId = submissionId, SectionId = param.SectionId, SectionName = param.SectionName });
            return RedirectToRoute("CheckAnswersRoute", new { sectionSlug = param.SectionSlug });
        }
        else
        {
            TempData[TempDataConstants.Questions] = SerialiseParameter(new TempDataQuestions() { QuestionRef = nextQuestionId, SubmissionId = submissionId });
            var question = await _getQuestionQuery.GetQuestionById(nextQuestionId);
            return RedirectToRoute("SectionQuestionAnswer", new { sectionSlug = param.SectionSlug, question = question?.Slug });
        }
    }
}
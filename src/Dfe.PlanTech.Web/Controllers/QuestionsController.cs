using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Questions.Models;
using Dfe.PlanTech.Domain.Responses.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
[Route("/question")]
public class QuestionsController : BaseController<QuestionsController>
{
    private readonly GetQuestionQuery _getQuestionnaireQuery;
    private readonly IGetQuestionQuery _getQuestionQuery;
    private readonly IRecordQuestionCommand _recordQuestionCommand;
    private readonly IRecordAnswerCommand _recordAnswerCommand;
    private readonly ICreateSubmissionCommand _createSubmissionCommand;
    private readonly ICreateResponseCommand _createResponseCommand;
    private readonly IGetResponseQuery _getResponseQuery;
    private readonly IUser _user;

    public QuestionsController(
        ILogger<QuestionsController> logger,
        IUrlHistory history,
        [FromServices] GetQuestionQuery getQuestionnaireQuery,
        [FromServices] IGetQuestionQuery getQuestionQuery,
        [FromServices] IRecordQuestionCommand recordQuestionCommand,
        [FromServices] IRecordAnswerCommand recordAnswerCommand,
        [FromServices] ICreateSubmissionCommand createSubmissionCommand,
        [FromServices] ICreateResponseCommand createResponseCommand,
        [FromServices] IGetResponseQuery getResponseQuery,
        IUser user) : base(logger, history)
    {
        _getQuestionnaireQuery = getQuestionnaireQuery;
        _getQuestionQuery = getQuestionQuery;
        _recordQuestionCommand = recordQuestionCommand;
        _recordAnswerCommand = recordAnswerCommand;
        _createSubmissionCommand = createSubmissionCommand;
        _createResponseCommand = createResponseCommand;
        _getResponseQuery = getResponseQuery;
        _user = user;
    }

    [HttpGet("{id?}")]
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="section">Name of current section (if starting new)</param>
    /// <param name="query"></param>
    /// <exception cref="ArgumentNullException">Throws exception when Id is null or empty</exception>
    /// <returns></returns>
    public async Task<IActionResult> GetQuestionById(string id, string? section, int? submissionId, string? answerRef, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        object? parameters;

        TempData.TryGetValue("param", out parameters);

        var question = await _GetQuestion(id, section, cancellationToken);

        var viewModel = new QuestionViewModel()
        {
            Question = question,
            AnswerRef = answerRef,
            BackUrl = history.LastVisitedUrl?.ToString() ?? "self-assessment",
            Params = parameters != null ? parameters.ToString() : null,
            SubmissionId = submissionId,
        };

        return View("Question", viewModel);
    }

    [HttpPost("SubmitAnswer")]
    public async Task<IActionResult> SubmitAnswer(SubmitAnswerDto submitAnswerDto)
    {
        int submissionId;
        if (submitAnswerDto == null) throw new ArgumentNullException(nameof(submitAnswerDto));

        if (!ModelState.IsValid) return RedirectToAction("GetQuestionById", new { id = submitAnswerDto.QuestionId, submissionId = submitAnswerDto.SubmissionId });
        Params param = new Params();

        var userId = Convert.ToUInt16(await _user.GetCurrentUserId());
        var establishmentId = Convert.ToUInt16(await _user.GetEstablishmentId());

        if (!string.IsNullOrEmpty(submitAnswerDto.Params))
        {
            param = ParseParameters(submitAnswerDto.Params) ?? null!;
            TempData["param"] = submitAnswerDto.Params;
        }

        var questionId = await _RecordQuestion(new RecordQuestionDto() { QuestionText = await _GetQuestionTextById(submitAnswerDto.QuestionId), ContentfulRef = submitAnswerDto.QuestionId });
        var answerId = await _RecordAnswer(new RecordAnswerDto() { AnswerText = await _GetAnswerTextById(submitAnswerDto.QuestionId, submitAnswerDto.ChosenAnswerId), ContentfulRef = submitAnswerDto.ChosenAnswerId });

        if (submitAnswerDto.SubmissionId is null || submitAnswerDto.SubmissionId == 0)
        {
            submissionId = await _RecordSubmission(new Submission() { EstablishmentId = establishmentId, SectionId = param.SectionId, SectionName = param.SectionName });
        }
        else
        {
            submissionId = Convert.ToUInt16(submitAnswerDto.SubmissionId);
        }

        await _RecordResponse(new RecordResponseDto() { AnswerId = answerId, QuestionId = questionId, SubmissionId = submissionId, UserId = userId, Maturity = await _GetMaturityForAnswer(submitAnswerDto.QuestionId, submitAnswerDto.ChosenAnswerId) });

        string? nextQuestionId = await _GetNextQuestionId(submitAnswerDto.QuestionId, submitAnswerDto.ChosenAnswerId);

        if (string.IsNullOrEmpty(nextQuestionId) || await _NextQuestionIsAnswered(submissionId, nextQuestionId)) return RedirectToAction("CheckAnswersPage", "CheckAnswers", new { submissionId = submissionId, sectionName = param.SectionName });
        else return RedirectToAction("GetQuestionById", new { id = nextQuestionId, submissionId = submissionId });
    }

    private async Task<string?> _GetNextQuestionId(string questionId, string chosenAnswerId)
    {
        Domain.Questionnaire.Models.Question? question = await _GetQuestion(questionId, null, CancellationToken.None);

        foreach (Domain.Questionnaire.Models.Answer answer in question.Answers)
        {
            if (answer.Sys.Id.Equals(chosenAnswerId)) return answer.NextQuestion?.Sys.Id ?? null;
        }

        return null;
    }

    private async Task<bool> _NextQuestionIsAnswered(int submissionId, string nextQuestionId)
    {
        Response[]? responseList = await _GetResponseList(submissionId) ?? throw new NullReferenceException(nameof(responseList));

        foreach (Response response in responseList)
        {
            Domain.Questions.Models.Question? question = await _GetResponseQuestion(response.QuestionId) ?? throw new NullReferenceException(nameof(question));
            if (question.ContentfulRef.Equals(nextQuestionId)) return true;
        }

        return false;
    }

    private static Params? ParseParameters(string parameters)
    {
        if (string.IsNullOrEmpty(parameters))
            return null;

        var splitParams = parameters.Split('+');

        if (splitParams is null)
            return null;

        return new Params
        {
            SectionName = splitParams.Length > 0 ? splitParams[0].ToString() : string.Empty,
            SectionId = splitParams.Length > 1 ? splitParams[1].ToString() : string.Empty,
        };
    }

    private async Task<Response[]?> _GetResponseList(int submissionId)
    {
        return await _getResponseQuery.GetResponseListBy(submissionId);
    }

    private async Task<Domain.Questions.Models.Question?> _GetResponseQuestion(int questionId)
    {
        return await _getQuestionQuery.GetQuestionBy(questionId);
    }

    private async Task<string?> _GetQuestionTextById(String questionId)
    {
        var question = await _GetQuestion(questionId, null, CancellationToken.None);
        return question.Text;
    }

    private async Task<string?> _GetAnswerTextById(string questionId, string chosenAnswerId)
    {
        var question = await _GetQuestion(questionId, null, CancellationToken.None);
        foreach (var answer in question.Answers)
        {
            if (answer.Sys?.Id == chosenAnswerId) return answer.Text;
        }
        return null;
    }

    private async Task<string> _GetMaturityForAnswer(string questionId, string chosenAnswerId)
    {
        if (string.IsNullOrEmpty(questionId) || string.IsNullOrEmpty(chosenAnswerId))
            return string.Empty;

        var question = await _GetQuestion(questionId, null, CancellationToken.None);

        var answer = question.Answers.FirstOrDefault(x => x.Sys?.Id == chosenAnswerId);
        if (answer is null)
            return string.Empty;

        return answer.Maturity;
    }
    private async Task<int> _RecordQuestion(RecordQuestionDto recordQuestionDto)
    {
        if (recordQuestionDto.QuestionText == null) throw new ArgumentNullException(nameof(recordQuestionDto));
        return await _recordQuestionCommand.RecordQuestion(recordQuestionDto);
    }

    private async Task<Domain.Questionnaire.Models.Question> _GetQuestion(string id, string? section, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        return await _getQuestionnaireQuery.GetQuestionById(id, section, cancellationToken) ?? throw new KeyNotFoundException($"Could not find question with id {id}");
    }

    private async Task<int> _RecordAnswer(RecordAnswerDto recordAnswerDto)
    {
        if (recordAnswerDto.AnswerText == null) throw new ArgumentNullException(nameof(recordAnswerDto));
        return await _recordAnswerCommand.RecordAnswer(recordAnswerDto);
    }

    private async Task<int> _RecordSubmission(Submission submission)
    {
        if (submission == null) throw new ArgumentNullException(nameof(submission));
        return await _createSubmissionCommand.CreateSubmission(submission);
    }

    private async Task _RecordResponse(RecordResponseDto recordResponseDto)
    {
        if (recordResponseDto == null) throw new ArgumentNullException(nameof(recordResponseDto));
        await _createResponseCommand.CreateResponse(recordResponseDto);
    }
}
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interface;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Exceptions;
using Dfe.PlanTech.Web.Middleware;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class QuestionsController : BaseController<QuestionsController>
{
    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IGetLatestResponsesQuery _getResponseQuery;
    private readonly UserProgressValidator _userProgressValidator;
    private readonly IUser _user;
    public QuestionsController(ILogger<QuestionsController> logger,
                                IGetSectionQuery getSectionQuery,
                                IGetLatestResponsesQuery getResponseQuery,
                                UserProgressValidator userProgressValidator,
                                IUser user) : base(logger)
    {
        _getResponseQuery = getResponseQuery;
        _getSectionQuery = getSectionQuery;
        _userProgressValidator = userProgressValidator;
        _user = user;
    }

    [HttpGet("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> GetQuestionBySlug(string sectionSlug,
                                                        string questionSlug,
                                                        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(questionSlug)) throw new ArgumentNullException(nameof(questionSlug));

        var journeyStatus = await _userProgressValidator.GetJourneyStatusForSection(sectionSlug, cancellationToken);

        switch (journeyStatus.Status)
        {
            case JourneyStatus.Completed:
                {
                    var question = journeyStatus.Section.Questions.FirstOrDefault(question => question.Slug == questionSlug) ?? throw new ContentfulDataUnavailableException($"Couldn't find question with slug {questionSlug} under section {sectionSlug}");
                    return RenderView(GenerateViewModel(sectionSlug, question, journeyStatus.Section, journeyStatus.LastResponseAnswerContentfulId));
                }
            case JourneyStatus.CheckAnswers:
                {
                    var question = journeyStatus.Section.Questions.FirstOrDefault(q => q.Slug == questionSlug) ??
                                    throw new ContentfulDataUnavailableException("No");

                    var viewModel = GenerateViewModel(sectionSlug, question, journeyStatus.Section, null);
                    return RenderView(viewModel);
                }
            case JourneyStatus.NextQuestion:
            case JourneyStatus.NotStarted:
                {
                    if (journeyStatus.NextQuestion == null)
                    {
                        throw new InvalidDataException("Next question is null but really shouldn't be");
                    }

                    if (journeyStatus.NextQuestion!.Slug != questionSlug)
                    {
                        return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug = journeyStatus.NextQuestion!.Slug });
                    }

                    var viewModel = GenerateViewModel(sectionSlug, journeyStatus.NextQuestion!, journeyStatus.Section, null);
                    return RenderView(viewModel);
                }

        }

        throw new Exception($"Invalid journey state");
    }


    [HttpGet("{sectionSlug}/next-question")]
    public async Task<IActionResult> GetNextUnansweredQuestion(string sectionSlug,
                                                                [FromServices] IGetNextUnansweredQuestionQuery getQuestionQuery,
                                                                CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));

        var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                        throw new KeyNotFoundException($"Could not find section with slug {sectionSlug}");

        int establishmentId = await _user.GetEstablishmentId();

        var nextQuestion = await getQuestionQuery.GetNextUnansweredQuestion(establishmentId, section, cancellationToken);

        if (nextQuestion == null) return this.RedirectToCheckAnswers(sectionSlug);

        return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug = nextQuestion!.Slug });
    }

    [HttpPost("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(string sectionSlug, string questionSlug, SubmitAnswerDto submitAnswerDto, [FromServices] ISubmitAnswerCommand submitAnswerCommand, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = await GenerateViewModel(sectionSlug, questionSlug, cancellationToken);
            viewModel.ErrorMessages = ModelState.Values.SelectMany(value => value.Errors.Select(err => err.ErrorMessage)).ToArray();
            return RenderView(viewModel);
        }

        try
        {
            await submitAnswerCommand.SubmitAnswer(submitAnswerDto, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError("An error has occurred while submitting an answer with the following message: {} ", e.Message);
            var viewModel = await GenerateViewModel(sectionSlug, questionSlug, cancellationToken);
            viewModel.ErrorMessages = new[] { "Save failed. Please try again later." };
            return RenderView(viewModel);
        }

        return RedirectToAction(nameof(GetNextUnansweredQuestion), new { sectionSlug });
    }

    private IActionResult RenderView(QuestionViewModel viewModel) => View("Question", viewModel);

    private async Task<QuestionViewModel> GenerateViewModel(string sectionSlug, string questionSlug, CancellationToken cancellationToken)
    {
        var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                        throw new KeyNotFoundException($"Could not find section with slug {sectionSlug}");

        var question = section.Questions.FirstOrDefault(question => question.Slug == questionSlug) ??
                            throw new KeyNotFoundException($"Could not find question slug {questionSlug} under section {sectionSlug}");

        int establishmentId = await _user.GetEstablishmentId();

        var latestResponseForQuestion = await _getResponseQuery.GetLatestResponseForQuestion(establishmentId,
                                                                                section.Sys.Id,
                                                                                question.Sys.Id,
                                                                                cancellationToken);

        return GenerateViewModel(sectionSlug, question, section, latestResponseForQuestion?.AnswerRef);
    }

    private QuestionViewModel GenerateViewModel(string sectionSlug, Question question, ISection section, string? latestAnswerContentfulId)
    {
        ViewData["Title"] = question.Text;

        return new QuestionViewModel()
        {
            Question = question,
            AnswerRef = latestAnswerContentfulId,
            SectionName = section.Name,
            SectionSlug = sectionSlug,
            SectionId = section.Sys.Id
        };
    }


}
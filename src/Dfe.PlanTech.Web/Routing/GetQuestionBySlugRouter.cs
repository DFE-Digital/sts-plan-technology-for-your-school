using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public class GetQuestionBySlugRouter : IGetQuestionBySlugRouter
{
    private readonly IGetLatestResponsesQuery _getResponseQuery;
    private readonly IUser _user;
    private readonly ISubmissionStatusProcessor _router;

    public GetQuestionBySlugRouter(IGetLatestResponsesQuery getResponseQuery, IUser user, ISubmissionStatusProcessor router)
    {
        _getResponseQuery = getResponseQuery;
        _user = user;
        _router = router;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// If we're at the next question, just show the page.
    /// Otherwise, if the section isn't started or completed, reroute to interstitial page
    /// Otherwise, we are at the "Check Answers" or "Next Question" status
    /// If the question is attached (i.e. answered in current user journey)?
    /// Not attached? Show either Check Answers page (if check answers status), or reroute to Next Question in journey
    /// </remarks>
    /// <param name="sectionSlug"></param>
    /// <param name="questionSlug"></param>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<IActionResult> ValidateRoute(string sectionSlug, string questionSlug, QuestionsController controller, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(questionSlug)) throw new ArgumentNullException(nameof(questionSlug));

        await _router.GetJourneyStatusForSection(sectionSlug, cancellationToken);

        if (IsSlugForNextQuestion(questionSlug))
        {
            var viewModel = controller.GenerateViewModel(sectionSlug, _router.NextQuestion!, _router.Section, null);
            return controller.RenderView(viewModel);
        }

        if (SectionIsAtStart) return PageRedirecter.RedirectToInterstitialPage(controller, sectionSlug);

        return await ProcessOtherStatuses(sectionSlug, questionSlug, controller, cancellationToken);
    }

    /// <summary>
    /// Handle any other section status other than:
    /// 1. Question slug is next question
    /// 2. OR is start of section
    /// </summary>
    /// <remarks>
    /// Checks to see if the question is part of latest user responses.
    /// If so -> show page
    /// If not ->
    ///   - Redirect to check answers page if on "check answers" status
    ///   OR
    ///   - Redirect to next question if on "next question" status
    /// </remarks>
    /// <param name="sectionSlug"></param>
    /// <param name="questionSlug"></param>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<IActionResult> ProcessOtherStatuses(string sectionSlug, string questionSlug, QuestionsController controller, CancellationToken cancellationToken)
    {
        var question = GetQuestionForSlug(questionSlug);

        var responses = await GetLatestResponsesForSection(cancellationToken);

        var isAttachedQuestion = IsQuestionAttached(responses, question);

        if (!isAttachedQuestion) return HandleNotAttachedQuestion(sectionSlug, controller);

        var latestResponseForQuestion = GetLatestResponseForQuestion(responses, question);

        var viewModel = controller.GenerateViewModel(sectionSlug,
                                                     question,
                                                     _router.Section,
                                                     latestResponseForQuestion.AnswerRef);

        return controller.RenderView(viewModel);
    }

    /// <summary>
    /// Retrieves matching question from the <see cref="Section"/> object in <see cref="_router"/> 
    /// </summary>
    /// <param name="questionSlug">Question slug to look for</param>
    /// <returns>Found question</returns>
    /// <exception cref="ContentfulDataUnavailableException">Thrown if no matching question is found</exception>
    private Question GetQuestionForSlug(string questionSlug)
    => _router.Section.Questions.FirstOrDefault(q => q.Slug == questionSlug) ?? throw new ContentfulDataUnavailableException($"Could not find question '{questionSlug}'");

    /// <summary>
    /// Retrieves latest responses for the <see cref="Section"/> object in <see cref="_router"/> 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="DatabaseException">Thrown if no responses are found</exception>
    private async Task<List<QuestionWithAnswer>> GetLatestResponsesForSection(CancellationToken cancellationToken)
    {
        var establishmentId = await _user.GetEstablishmentId();

        var latestResponses = await _getResponseQuery.GetLatestResponses(establishmentId, _router.Section.Sys.Id, false, cancellationToken);

        if (latestResponses == null || latestResponses.Responses.Count == 0) throw new DatabaseException($"Could not find latest responses for '{_router.Section.Sys.Id}'");

        return latestResponses.Responses;
    }

    /// <summary>
    /// Finds the latest response in the collection for the desired question
    /// </summary>
    /// <param name="question">Question to find latest response for</param>
    /// <param name="responses">Latest responses for the section</param>
    /// <returns></returns>
    /// <exception cref="DatabaseException">Thrown if none found</exception>
    private static QuestionWithAnswer GetLatestResponseForQuestion(IEnumerable<QuestionWithAnswer> responses, Question question)
    => responses.FirstOrDefault(response => response.QuestionRef == question.Sys.Id) ??
       throw new DatabaseException($"Could not find response for question '{question.Sys.Id}'");


    /// <summary>
    /// is the question attached (i.e. part of the latest response path for establishment)?
    /// </summary>
    /// <param name="responses">Latest response path for the <see cref="Section"/> from the <see cref="_router"/></param>
    /// <param name="question">Question to check attached status</param>
    /// <returns></returns>
    private bool IsQuestionAttached(IEnumerable<QuestionWithAnswer> responses, Question question)
    => _router.Section.GetOrderedResponsesForJourney(responses).Any(response => response.QuestionRef == question.Sys.Id);

    /// <summary>
    /// Returns an IActionResult depending on the <see cref="SubmissionStatus"/> in the <see cref="_router"/>
    /// </summary>
    /// <remarks>
    /// If CheckAnswers status -> reroutes to Check Answers page for section
    /// If NextQuestion status -> Redirect to next question for section
    /// Otherwise -> Throw exception as something has gone terribly wrong
    /// </remarks>
    /// <param name="sectionSlug"></param>
    /// <param name="controller"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    private IActionResult HandleNotAttachedQuestion(string sectionSlug, QuestionsController controller)
    => _router.Status switch
    {
        SubmissionStatus.CheckAnswers => controller.RedirectToCheckAnswers(sectionSlug),
        SubmissionStatus.NextQuestion => QuestionsController.RedirectToQuestionBySlug(sectionSlug, _router.NextQuestion?.Slug ?? throw new InvalidDataException("NextQuestion is null"), controller),
        _ => throw new InvalidDataException("Should not be here"),
    };

    /// <summary>
    /// Are we at the start of the section? I.e. "NotStarted" or "Completed"
    /// </summary>
    private bool SectionIsAtStart => _router.Status == SubmissionStatus.NotStarted || _router.Status == SubmissionStatus.Completed;

    /// <summary>
    /// Does this slug match the NextQuestion field in the <see cref="_router"/> 
    /// </summary>
    /// <param name="questionSlug"></param>
    /// <returns></returns>
    private bool IsSlugForNextQuestion(string questionSlug)
    => _router.NextQuestion != null && _router.NextQuestion!.Slug == questionSlug;
}

using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public class GetRecommendationRouter : IGetRecommendationRouter
{
    private readonly ISubmissionStatusProcessor _router;
    private readonly IGetLatestResponsesQuery _getLatestResponsesQuery;
    private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery;

    public GetRecommendationRouter(ISubmissionStatusProcessor router,
                                   IGetLatestResponsesQuery getLatestResponsesQuery,
                                   IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery)
    {
        _router = router;
        _getLatestResponsesQuery = getLatestResponsesQuery;
        _getSubTopicRecommendationQuery = getSubTopicRecommendationQuery;
    }

    public async Task<IActionResult> ValidateRoute(
        string sectionSlug,
        string recommendationSlug,
        bool checklist,
        RecommendationsController controller,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(recommendationSlug))
            throw new ArgumentNullException(nameof(recommendationSlug));

        await _router.GetJourneyStatusForSectionRecommendation(sectionSlug, cancellationToken);

        return _router.Status switch
        {
            SubmissionStatus.Completed => checklist ?
                await HandleChecklist(controller, recommendationSlug, cancellationToken) :
                await HandleCompleteStatus(controller, recommendationSlug, cancellationToken),
            SubmissionStatus.CheckAnswers => controller.RedirectToCheckAnswers(sectionSlug),
            SubmissionStatus.NextQuestion => HandleQuestionStatus(sectionSlug, controller),
            SubmissionStatus.NotStarted => PageRedirecter.RedirectToSelfAssessment(controller),
            _ => throw new InvalidOperationException($"Invalid journey status - {_router.Status}"),
        };
    }

    public async Task<IActionResult> GetRecommendationPreview(string sectionSlug,
                                                              string? maturity,
                                                              RecommendationsController controller,
                                                              CancellationToken cancellationToken)
    {
        await _router.GetJourneyStatusForSectionRecommendation(sectionSlug, cancellationToken);
        var recommendation = await _getSubTopicRecommendationQuery.GetSubTopicRecommendation(_router.Section.Sys.Id, cancellationToken) ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for:  {_router.Section.Name}");

        var intro = recommendation.Intros.Find(intro => string.Equals(intro.Maturity, maturity, StringComparison.InvariantCultureIgnoreCase)) ?? recommendation.Intros[0];

        var viewModel = new RecommendationsViewModel()
        {
            SectionName = _router.Section.Name,
            Intro = intro,
            Chunks = recommendation.Section.Chunks,
            Slug = "preview",
        };

        return controller.View("~/Views/Recommendations/Recommendations.cshtml", viewModel);
    }

    private async Task<(SubtopicRecommendation, RecommendationIntro, List<RecommendationChunk>, IEnumerable<QuestionWithAnswer>)> GetSubtopicRecommendation(CancellationToken cancellationToken)
    {
        if (_router.SectionStatus?.Maturity == null)
            throw new DatabaseException("Maturity is null, but shouldn't be for a completed section");

        if (_router.Section == null)
            throw new DatabaseException("Section is null, but shouldn't be.");

        var submissionResponses = await _getLatestResponsesQuery.GetLatestResponses(await _router.User.GetEstablishmentId(), _router.Section.Sys.Id, true, cancellationToken) ?? throw new DatabaseException($"Could not find users answers for:  {_router.Section.Name}");
        var latestResponses = _router.Section.GetOrderedResponsesForJourney(submissionResponses.Responses);

        var subTopicRecommendation = await _getSubTopicRecommendationQuery.GetSubTopicRecommendation(_router.Section.Sys.Id, cancellationToken) ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for:  {_router.Section.Name}");

        var subTopicIntro = subTopicRecommendation.GetRecommendationByMaturity(_router.SectionStatus.Maturity) ?? throw new ContentfulDataUnavailableException($"Could not find recommendation intro for maturity:  {_router.SectionStatus?.Maturity}");

        var subTopicChunks = subTopicRecommendation.Section.GetRecommendationChunksByAnswerIds(latestResponses.Select(answer => answer.AnswerRef));

        return (subTopicRecommendation, subTopicIntro, subTopicChunks, latestResponses);
    }

    /// <summary>
    /// Fetch the model for the recommendation page (if correct recommendation for section + maturity),
    /// </summary>
    private async Task<RecommendationsViewModel> GetRecommendationViewModel(string recommendationSlug, CancellationToken cancellationToken)
    {
        var (subTopicRecommendation, subTopicIntro, subTopicChunks, latestResponses) = await GetSubtopicRecommendation(cancellationToken);

        return new RecommendationsViewModel()
        {
            SectionName = subTopicRecommendation.Subtopic.Name,
            Intro = subTopicIntro,
            Chunks = subTopicChunks,
            Slug = recommendationSlug,
            SubmissionResponses = latestResponses
        };
    }

    /// <summary>
    /// Render the recommendation page and mark the recommendation as viewed in the database
    /// </summary>
    private async Task<IActionResult> HandleCompleteStatus(RecommendationsController controller, string recommendationSlug, CancellationToken cancellationToken)
    {
        var viewModel = await GetRecommendationViewModel(recommendationSlug, cancellationToken);

        var establishmentId = await _router.User.GetEstablishmentId();
        await _getLatestResponsesQuery.ViewLatestSubmission(establishmentId, _router.Section.Sys.Id);

        return controller.View("~/Views/Recommendations/Recommendations.cshtml", viewModel);
    }

    /// <summary>
    /// Render the share recommendations checklist page
    /// </summary>
    private async Task<IActionResult> HandleChecklist(RecommendationsController controller, string recommendationSlug, CancellationToken cancellationToken)
    {
        var viewModel = await GetRecommendationViewModel(recommendationSlug, cancellationToken);

        return controller.View("~/Views/Recommendations/RecommendationsChecklist.cshtml", viewModel);
    }

    /// <summary>
    /// Redirect user to next question in their journey
    /// </summary>
    /// <param name="sectionSlug"></param>
    /// <param name="controller"></param>
    /// <returns></returns>
    private IActionResult HandleQuestionStatus(string sectionSlug, Controller controller)
    => QuestionsController.RedirectToQuestionBySlug(sectionSlug, _router.NextQuestion!.Slug, controller);
}

using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public class GetRecommendationRouter : IGetRecommendationRouter
{
    private readonly ISubmissionStatusProcessor _router;
    private readonly IGetLatestResponsesQuery _getLatestResponsesQuery;
    private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery;
    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IUser _user;
    private readonly IGetPageQuery _getPageQuery;

    public GetRecommendationRouter(ISubmissionStatusProcessor router,
                                   IGetLatestResponsesQuery getLatestResponsesQuery,
                                   IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery,
                                   IGetSectionQuery getSectionQuery,
                                   IGetPageQuery getPageQuery,
                                   IUser user)
    {
        _router = router;
        _getLatestResponsesQuery = getLatestResponsesQuery;
        _getSubTopicRecommendationQuery = getSubTopicRecommendationQuery;
        _getSectionQuery = getSectionQuery;
        _user = user;
        _getPageQuery = getPageQuery;
    }

    public async Task<IActionResult> ValidateRoute(
        string categorySlug,
        string sectionSlug,
        bool checklist,
        RecommendationsController controller,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        await _router.GetJourneyStatusForSectionRecommendation(sectionSlug, true, cancellationToken);

        return _router.Status switch
        {
            Status.CompleteReviewed => checklist ?
                await HandleChecklist(controller, categorySlug, sectionSlug, cancellationToken) :
                await HandleCompleteStatus(controller, categorySlug, sectionSlug, cancellationToken),
            Status.CompleteNotReviewed => controller.RedirectToCheckAnswers(sectionSlug),
            Status.InProgress => HandleQuestionStatus(sectionSlug, controller),
            Status.NotStarted => PageRedirecter.RedirectToHomepage(controller),
            _ => throw new InvalidOperationException($"Invalid journey status - {_router.Status}"),
        };
    }

    public async Task<IActionResult> GetRecommendationPreview(string sectionSlug,
                                                              string? maturity,
                                                              RecommendationsController controller,
                                                              CancellationToken cancellationToken)
    {
        await _router.GetJourneyStatusForSectionRecommendation(sectionSlug, cancellationToken: cancellationToken);
        var recommendation = await _getSubTopicRecommendationQuery.GetSubTopicRecommendation(_router.Section.Sys.Id, cancellationToken) ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for:  {_router.Section.Name}");

        var viewModel = new RecommendationsViewModel()
        {
            SectionName = _router.Section.Name,
            Chunks = recommendation.Section.Chunks,
            Slug = "preview",
        };

        return controller.View("~/Views/Recommendations/Recommendations.cshtml", viewModel);
    }

    public async Task<string> GetRecommendationSlugForSection(string sectionSlug, CancellationToken cancellationToken)
    {
        await _router.GetJourneyStatusForSectionRecommendation(sectionSlug, true, cancellationToken);
        var (_, subTopicIntro, _, _) = await GetSubtopicRecommendation(cancellationToken);
        return subTopicIntro.Slug;
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

    public async Task<(Section, RecommendationChunk, List<RecommendationChunk>)> GetSingleRecommendation(string sectionSlug, string recommendationSlug, RecommendationsController controller, CancellationToken cancellationToken)
    {
        var section = await _getSectionQuery.GetSectionBySlug(sectionSlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find section with slug: {sectionSlug}");
        var submissionResponses = await _getLatestResponsesQuery.GetLatestResponses(await _user.GetEstablishmentId(), section.Sys.Id, true)
            ?? throw new DatabaseException($"Could not find users answers for:  {section.Name}");
        var latestResponses = section.GetOrderedResponsesForJourney(submissionResponses.Responses).ToList();
        var subTopicRecommendation = await _getSubTopicRecommendationQuery.GetSubTopicRecommendation(section.Sys.Id)
            ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for:  {section.Name}");
        var allChunks = subTopicRecommendation.Section.GetRecommendationChunksByAnswerIds(latestResponses.Select(answer => answer.AnswerRef))
            ?? throw new ContentfulDataUnavailableException($"Could not find recommendation chunks for section: {section.Name}");
        var currentChunk = allChunks.FirstOrDefault(chunk => chunk.SlugifiedLinkText == recommendationSlug)
            ?? throw new ContentfulDataUnavailableException($"No recommendation chunk found with slug mathing: {recommendationSlug}");

        return (section, currentChunk, allChunks);
    }

    /// <summary>
    /// Fetch the model for the recommendation page (if correct recommendation for section + maturity),
    /// </summary>
    private async Task<RecommendationsViewModel> GetRecommendationViewModel(string categorySlug, string sectionSlug, bool isChecklist = false, CancellationToken cancellationToken = default)
    {
        var (subTopicRecommendation, subTopicIntro, subTopicChunks, latestResponses) = await GetSubtopicRecommendation(cancellationToken);
        var categoryLandingPage = await _getPageQuery.GetPageBySlug(categorySlug);
        var category = categoryLandingPage?.Content[0] as Category ?? throw new ContentfulDataUnavailableException($"No category landing page found for slug: {categorySlug}");

        var latestCompletionDate = new DateTime?();

        if (isChecklist)
        {
            var establishmentId = await _router.User.GetEstablishmentId();

            latestCompletionDate = await _getLatestResponsesQuery.GetLatestCompletionDate(establishmentId, _router.Section.Sys.Id, true);
        }

        return new RecommendationsViewModel()
        {
            CategoryName = category.Header.Text,
            SectionName = subTopicRecommendation.Subtopic.Name,
            Chunks = subTopicChunks,
            LatestCompletionDate = latestCompletionDate.HasValue
                                ? DateTimeFormatter.FormattedDateShort(latestCompletionDate.Value)
                                : null,
            SectionSlug = sectionSlug,
            SubmissionResponses = latestResponses,
        };
    }

    /// <summary>
    /// Render the recommendation page and mark the recommendation as viewed in the database
    /// </summary>
    private async Task<IActionResult> HandleCompleteStatus(RecommendationsController controller, string categorySlug, string sectionSlug, CancellationToken cancellationToken)
    {
        var viewModel = await GetRecommendationViewModel(categorySlug, sectionSlug, cancellationToken: cancellationToken);

        return controller.View("~/Views/Recommendations/Recommendations.cshtml", viewModel);
    }

    /// <summary>
    /// Render the share recommendations checklist page
    /// </summary>
    private async Task<IActionResult> HandleChecklist(RecommendationsController controller, string categorySlug, string sectionSlug, CancellationToken cancellationToken)
    {
        var viewModel = await GetRecommendationViewModel(categorySlug, sectionSlug, isChecklist: true, cancellationToken: cancellationToken);

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

using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[LogInvalidModelState]
[Authorize]
[Route("/")]
public class RecommendationsController(
    ILogger<RecommendationsController> logger,
    IRecommendationsViewBuilder recommendationsViewBuilder,
    IRecommendationService recommendationService,
    IContentfulService contentfulService,
    ISubmissionService submissionService,
    ICurrentUser currentUser
)
    : BaseController<RecommendationsController>(logger)
{
    private readonly IRecommendationsViewBuilder _recommendationsViewBuilder = recommendationsViewBuilder ?? throw new ArgumentNullException(nameof(recommendationsViewBuilder));
    private readonly IRecommendationService _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
    private readonly IContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    private readonly ISubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly ICurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

    public const string ControllerName = "Recommendations";
    public const string GetSingleRecommendationAction = nameof(GetSingleRecommendation);
    public const string UpdateRecommendationStatusAction = nameof(UpdateRecommendationStatus);

    [HttpGet("{categorySlug}/{sectionSlug}/recommendations/{chunkSlug}", Name = GetSingleRecommendationAction)]
    public async Task<IActionResult> GetSingleRecommendation(string categorySlug, string sectionSlug, string chunkSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(chunkSlug);

        return await _recommendationsViewBuilder.RouteToSingleRecommendation(this, categorySlug, sectionSlug, chunkSlug, false);
    }

    [HttpGet("{categorySlug}/{sectionSlug}/recommendations/print", Name = "GetRecommendationChecklist")]
    public async Task<IActionResult> GetRecommendationChecklist(string categorySlug, string sectionSlug)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);

        return await _recommendationsViewBuilder.RouteBySectionAndRecommendation(this, categorySlug, sectionSlug, true);
    }

    [HttpPost("{categorySlug}/{sectionSlug}/recommendations/{chunkSlug}/update-status")]
    public async Task<IActionResult> UpdateRecommendationStatus(
        string categorySlug,
        string sectionSlug,
        string chunkSlug,
        [FromForm] string selectedStatus)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(categorySlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionSlug);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(chunkSlug);

        if (string.IsNullOrWhiteSpace(selectedStatus))
        {
            TempData["StatusUpdateError"] = "Select a status";
            return await _recommendationsViewBuilder.RouteToSingleRecommendation(this, categorySlug, sectionSlug, chunkSlug, false);
        }

        var selectedStatusDisplayName = RecommendationConstants.StatusDisplayNamesNonBreakingSpaces.GetValueOrDefault(selectedStatus, selectedStatus);

        // Allow only specific statuses
        if (!RecommendationConstants.ValidStatusKeys.Contains(selectedStatus))
        {
            Logger.LogWarning("Invalid / unrecognised status value received: {SelectedStatus}: {SelectedStatusDisplayName}", selectedStatus, selectedStatusDisplayName);
            TempData["StatusUpdateError"] = "Select a valid status";
            return await _recommendationsViewBuilder.RouteToSingleRecommendation(this, categorySlug, sectionSlug, chunkSlug, false);
        }


        var establishmentId = GetEstablishmentIdOrThrowException();
        var userId = GetUserIdOrThrowException();

        var section = await _contentfulService.GetSectionBySlugAsync(sectionSlug, includeLevel: 2)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, section, isCompletedSubmission: true);

        var answerIds = submissionRoutingData.Submission!.Responses.Select(r => r.AnswerSysId);
        var recommendationChunks = section.GetRecommendationChunksByAnswerIds(answerIds);

        var currentRecommendationChunk = recommendationChunks.FirstOrDefault(chunk => chunk.SlugifiedLinkText == chunkSlug)
           ?? throw new ContentfulDataUnavailableException($"No recommendation chunk found with slug matching: {chunkSlug}");

        await _recommendationService.UpdateRecommendationStatusAsync(
            currentRecommendationChunk.Id,
            establishmentId,
            userId,
            selectedStatus,
            $"Change reason: Status manually updated to {selectedStatus}"
        );

        // Set success message for the banner
        TempData["StatusUpdateSuccessTitle"] = $"Status updated to '{selectedStatusDisplayName}'";

        // Redirect back to the single recommendation page
        return await _recommendationsViewBuilder.RouteToSingleRecommendation(this, categorySlug, sectionSlug, chunkSlug, false);
    }

    private int GetEstablishmentIdOrThrowException()
    {
        var establishmentId = _currentUser.EstablishmentId;
        if (establishmentId == null)
        {
            throw new InvalidOperationException("Establishment ID is required but not available");
        }
        return establishmentId.Value;
    }

    private int GetUserIdOrThrowException()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
        {
            throw new InvalidOperationException("User ID is required but not available");
        }
        return userId.Value;
    }
}

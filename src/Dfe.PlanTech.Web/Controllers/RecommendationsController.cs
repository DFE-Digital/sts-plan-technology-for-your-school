using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Helpers;
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
    IContentfulService contentfulService,
    IRecommendationService recommendationService,
    IRecommendationsViewBuilder recommendationsViewBuilder,
    ISubmissionService submissionService,
    ICurrentUser currentUser
)
    : BaseController<RecommendationsController>(logger)
{
    private readonly ISubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly IRecommendationsViewBuilder _recommendationsViewBuilder = recommendationsViewBuilder ?? throw new ArgumentNullException(nameof(recommendationsViewBuilder));
    private readonly IRecommendationService _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
    private readonly IContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
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

        var selectedStatusDisplayName = selectedStatus.GetRecommendationStatusEnumValue();

        // Allow only specific statuses
        if (selectedStatusDisplayName is null)
        {
            Logger.LogWarning("Invalid / unrecognised status value received: {SelectedStatus}: {SelectedStatusDisplayName}", selectedStatus, selectedStatusDisplayName);
            TempData["StatusUpdateError"] = "Select a valid status";
            return await _recommendationsViewBuilder.RouteToSingleRecommendation(this, categorySlug, sectionSlug, chunkSlug, false);
        }

        var establishmentId = await GetActiveEstablishmentIdOrThrowException();
        var userId = GetUserIdOrThrowException();
        var userOrganisationId = _currentUser.UserOrganisationId;

        var section = await _contentfulService.GetSectionBySlugAsync(sectionSlug, includeLevel: 2)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, section, isCompletedSubmission: true);

        var answerIds = submissionRoutingData.Submission!.Responses.Select(r => r.AnswerSysId);
        var recommendationChunks = section.CoreRecommendations.ToList();

        var currentRecommendationChunk = recommendationChunks.FirstOrDefault(chunk => chunk.SlugifiedLinkText == chunkSlug)
           ?? throw new ContentfulDataUnavailableException($"No recommendation chunk found with slug matching: {chunkSlug}");

        await _recommendationService.UpdateRecommendationStatusAsync(
            currentRecommendationChunk.Id,
            establishmentId,
            userId,
            selectedStatus,
            $"Change reason: Status manually updated to '{selectedStatusDisplayName.Value.GetDisplayName()}'",
            userOrganisationId
        );

        // Set success message for the banner
        TempData["StatusUpdateSuccessTitle"] = $"Status updated to '{selectedStatusDisplayName.Value.GetDisplayName()}'";

        // Redirect back to the single recommendation page
        return await _recommendationsViewBuilder.RouteToSingleRecommendation(this, categorySlug, sectionSlug, chunkSlug, false);
    }



    /// <summary>
    ///
    /// </summary>
    /// <returns>The PlanTech database ID for the user's organisation (e.g. establishment, or establishment group)</returns>
    /// <exception cref="InvalidDataException"></exception>
    protected int GetUserOrganisationIdOrThrowException()
    {
        return _currentUser.UserOrganisationId ?? throw new InvalidDataException(nameof(_currentUser.UserOrganisationId));
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>The PlanTech database ID for the selected establishment (e.g. an establishment that a MAT user has selected)</returns>
    /// <exception cref="InvalidDataException"></exception>
    protected async Task<int> GetActiveEstablishmentIdOrThrowException()
    {
        return await _currentUser.GetActiveEstablishmentIdAsync() ?? throw new InvalidDataException(nameof(_currentUser.GetActiveEstablishmentIdAsync));
    }

    protected int GetUserIdOrThrowException()
    {
        return _currentUser.UserId ?? throw new InvalidOperationException("User ID is required but not available");
    }
}

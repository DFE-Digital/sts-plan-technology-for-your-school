using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class RecommendationsViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    ICurrentUser currentUser,
    INotifyService notifyService,
    IRecommendationService recommendationService,
    ISubmissionService submissionService,
    IMicrocopyProvider microcopyProvider
) : BaseViewBuilder(logger, contentfulService, currentUser), IRecommendationsViewBuilder
{
    private readonly INotifyService _notifyService =
        notifyService ?? throw new ArgumentNullException(nameof(notifyService));
    private readonly IRecommendationService _recommendationService =
        recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly IMicrocopyProvider _microcopyProvider =
        microcopyProvider ?? throw new ArgumentNullException(nameof(microcopyProvider));

    private const string RecommendationsChecklistViewName = "RecommendationsChecklist";
    private const string RecommendationsViewName = "Recommendations";
    private const string SingleRecommendationViewName = "SingleRecommendation";

    public async Task<IActionResult> RouteToSingleRecommendation(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string chunkSlug,
        bool useChecklist
    )
    {
        var establishmentId = await GetActiveEstablishmentIdOrThrowException();
        var categoryHeaderText =
            await ContentfulService.GetCategoryHeaderTextBySlugAsync(categorySlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find category header text for slug {categorySlug}"
            );
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug, includeLevel: 2)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );

        var recommendationChunks = section.CoreRecommendations.ToList();

        var currentRecommendationChunk =
            recommendationChunks.FirstOrDefault(chunk => chunk.Slug == chunkSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"No recommendation chunk found with slug matching: {chunkSlug}"
            );

        var currentRecommendationHistory =
            await _recommendationService.GetLatestRecommendationHistoryAsync(
                currentRecommendationChunk.Id,
                establishmentId
            );

        var currentRecommendationIndex = recommendationChunks.IndexOf(currentRecommendationChunk);
        var previousRecommendationChunk =
            currentRecommendationIndex > 0
                ? recommendationChunks[currentRecommendationIndex - 1]
                : null;
        var nextRecommendationChunk =
            currentRecommendationIndex != recommendationChunks.Count - 1
                ? recommendationChunks[currentRecommendationIndex + 1]
                : null;

        var recommendationHistory = await _recommendationService.GetRecommendationHistoryAsync(
            currentRecommendationChunk.Id,
            establishmentId
        );

        var orderedHistory = recommendationHistory.OrderByDescending(rh => rh.DateCreated).ToList();

        var groupedHistory = orderedHistory
            .GroupBy(rh => $"{rh.DateCreated.Date:MMMM yyyy}")
            .ToDictionary(group => group.Key, group => group.Select(g => g));

        var firstActivity =
            await _recommendationService.GetFirstActivityForEstablishmentRecommendationAsync(
                establishmentId,
                currentRecommendationChunk.Id
            );

        var viewModel = new SingleRecommendationViewModel
        {
            CategoryName = categoryHeaderText,
            CategorySlug = categorySlug,
            SectionSlug = sectionSlug,
            Section = section,
            Chunks = recommendationChunks,
            CurrentChunk = currentRecommendationChunk,
            PreviousChunk = previousRecommendationChunk,
            NextChunk = nextRecommendationChunk,
            CurrentChunkPosition = currentRecommendationIndex + 1,
            TotalChunks = recommendationChunks.Count,
            SelectedStatusKey =
                currentRecommendationHistory?.NewStatus ?? RecommendationStatus.NotStarted,
            LastUpdated = currentRecommendationHistory?.DateCreated,
            SuccessMessageTitle = controller.TempData["StatusUpdateSuccessTitle"] as string,
            StatusErrorMessage = controller.TempData["StatusUpdateError"] as string,
            StatusOptions = Enum.GetValues<RecommendationStatus>()
                .ToDictionary(key => key, key => key.GetDisplayName()),
            OriginatingSlug = chunkSlug,
            History = groupedHistory,
            FirstActivity = firstActivity,
        };

        return controller.View(SingleRecommendationViewName, viewModel);
    }

    public async Task<IActionResult> RouteBySectionAndRecommendation(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        bool useChecklist,
        string? singleChunkSlug,
        string? originatingSlug
    )
    {
        var establishmentId = await GetActiveEstablishmentIdOrThrowException();
        var category =
            await ContentfulService.GetCategoryBySlugAsync(categorySlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find category for slug {categorySlug}"
            );
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(
            establishmentId,
            section,
            status: SubmissionStatus.CompleteReviewed
        );

        switch (submissionRoutingData.Status)
        {
            case SubmissionStatus.NotStarted:
                return controller.RedirectToHomePage();

            case SubmissionStatus.InProgress:
                return controller.RedirectToAction(
                    nameof(QuestionsController.GetQuestionBySlug),
                    nameof(QuestionsController).GetControllerNameSlug(),
                    new
                    {
                        categorySlug,
                        sectionSlug,
                        submissionRoutingData.NextQuestion!.Slug,
                    }
                );

            case SubmissionStatus.CompleteNotReviewed:
                return controller.RedirectToCheckAnswers(categorySlug, sectionSlug);

            case SubmissionStatus.CompleteReviewed:
                var viewModel = await BuildRecommendationsViewModel(
                    category,
                    section,
                    submissionRoutingData,
                    sectionSlug,
                    categorySlug
                );

                var viewName = useChecklist
                    ? RecommendationsChecklistViewName
                    : RecommendationsViewName;

                if (!string.IsNullOrWhiteSpace(singleChunkSlug))
                {
                    var selectedChunk =
                        viewModel.Chunks.FirstOrDefault(c => c.Slug == singleChunkSlug)
                        ?? throw new ContentfulDataUnavailableException(
                            $"No recommendation chunk found with slug matching: {singleChunkSlug}"
                        );

                    viewModel = new RecommendationsViewModel
                    {
                        CategoryName = viewModel.CategoryName,
                        CategorySlug = viewModel.CategorySlug,
                        SectionName = viewModel.SectionName,
                        SectionSlug = viewModel.SectionSlug,
                        Chunks = [selectedChunk],
                        CurrentChunkCount = viewModel.Chunks.IndexOf(selectedChunk) + 1,
                        TotalChunks = viewModel.Chunks.Count,
                        LatestCompletionDate = viewModel.LatestCompletionDate,
                        SubmissionResponses = viewModel.SubmissionResponses,
                        OriginatingSlug = singleChunkSlug,
                    };
                }
                else
                {
                    viewModel.OriginatingSlug = originatingSlug;
                }

                return controller.View(viewName, viewModel);

            default:
                throw new InvalidOperationException(
                    $"Invalid journey status - {submissionRoutingData.Status}"
                );
        }
    }

    public async Task<IActionResult> UpdateRecommendationStatusAsync(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string chunkSlug,
        string? selectedStatus,
        string? notes
    )
    {
        var selectedStatusEnum = selectedStatus.GetRecommendationStatusEnumValue();

        // Allow only specific statuses
        if (selectedStatusEnum is null)
        {
            Logger.LogWarning(
                "Invalid / unrecognised status value received: {SelectedStatus}: {SelectedStatusDisplayName}",
                selectedStatus,
                selectedStatusEnum
            );
            controller.TempData["StatusUpdateError"] = "Select a valid status";
            return await RouteToSingleRecommendation(
                controller,
                categorySlug,
                sectionSlug,
                chunkSlug,
                false
            );
        }

        var establishmentId = await GetActiveEstablishmentIdOrThrowException();
        var userId = GetUserIdOrThrowException();
        var userOrganisationId = CurrentUser.UserOrganisationId;

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug, includeLevel: 2)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(
            establishmentId,
            section,
            status: SubmissionStatus.CompleteReviewed
        );

        var answerIds = submissionRoutingData.Submission!.Responses.Select(r => r.AnswerSysId);
        var recommendationChunks = section.CoreRecommendations.ToList();

        var currentRecommendationChunk =
            recommendationChunks.FirstOrDefault(chunk => chunk.Slug == chunkSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"No recommendation chunk found with slug matching: {chunkSlug}"
            );

        var dynamicValues = new Dictionary<string, string>
        {
            ["recStatus"] = selectedStatusEnum.Value.GetDisplayName()
        };

        await _recommendationService.UpdateRecommendationStatusAsync(
            currentRecommendationChunk.Id,
            establishmentId,
            userId,
            selectedStatusEnum.Value,
            notes
                ??
                await _microcopyProvider.GetTextByKeyAsync(ContentfulMicrocopyConstants.SingleRecommendationHistoryReason, dynamicValues),
            CurrentUser.IsMat ? userOrganisationId : null
        );

        // Set success message for the banner
        controller.TempData["StatusUpdateSuccessTitle"] =
            await _microcopyProvider.GetTextByKeyAsync(ContentfulMicrocopyConstants.SingleRecommendationSuccessHeader, dynamicValues);

        // Redirect back to the single recommendation page
        return PageRedirecter.RedirectToGetSingleRecommendation(
            controller,
            categorySlug,
            sectionSlug,
            chunkSlug
        );
    }

    [ExcludeFromCodeCoverage]
    public Task<IActionResult> RouteToPrintSingle(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string chunkSlug
    )
    {
        return RouteBySectionAndRecommendation(
            controller,
            categorySlug,
            sectionSlug,
            useChecklist: true,
            singleChunkSlug: chunkSlug,
            originatingSlug: chunkSlug
        );
    }

    [ExcludeFromCodeCoverage]
    public Task<IActionResult> RouteToPrintAll(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string chunkSlug
    )
    {
        return RouteBySectionAndRecommendation(
            controller,
            categorySlug,
            sectionSlug,
            useChecklist: true,
            singleChunkSlug: null,
            originatingSlug: chunkSlug
        );
    }

    public async Task<IActionResult> RouteToShareRecommendationAsync(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string chunkSlug,
        ShareByEmailInputViewModel? inputModel = null
    )
    {
        var category =
            await ContentfulService.GetCategoryBySlugAsync(categorySlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find category for slug {categorySlug}"
            );
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );
        var recommendationChunk =
            section.CoreRecommendations.FirstOrDefault(r =>
                r.Slug.Equals(chunkSlug, StringComparison.OrdinalIgnoreCase)
            )
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find chunk for slug {chunkSlug}"
            );

        var viewModel = BuildShareByEmailViewModel(
            nameof(RecommendationsController),
            nameof(RecommendationsController.ShareSingleRecommendation),
            category,
            recommendationChunk,
            categorySlug,
            sectionSlug,
            chunkSlug,
            inputModel
        );

        if (inputModel is null || !controller.ModelState.IsValid)
        {
            return controller.View(ShareByEmailViewName, viewModel);
        }

        var establishmentId = await GetActiveEstablishmentIdOrThrowException();
        var latestRecommendationHistory =
            await _recommendationService.GetLatestRecommendationHistoryAsync(
                recommendationChunk.Id,
                establishmentId
            );
        if (latestRecommendationHistory?.NewStatus is null)
        {
            throw new InvalidDataException("Cannot send an email without a recommendation status");
        }
        var textBodyId = recommendationChunk.Content[0].Id;
        var textBody =
            await ContentfulService.GetTextBodyByIdAsync(textBodyId)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find text body entry for id {textBodyId}"
            );

        var establishmentName =
            await CurrentUser.GetActiveEstablishmentNameAsync()
            ?? throw new InvalidDataException(
                "Cannot send an email without an active establishment name"
            );

        var notifySendResults = _notifyService.SendSingleRecommendationEmail(
            inputModel.ToModel(),
            textBody,
            establishmentName,
            recommendationChunk.HeaderText,
            section.Name,
            latestRecommendationHistory.NewStatus.Value
        );

        var returnToModel = new ActionViewModel(
            actionName: nameof(RecommendationsController.GetSingleRecommendation),
            controllerName: nameof(RecommendationsController).GetControllerNameSlug(),
            linkText: @$"Back to ""{recommendationChunk.HeaderText}""",
            routeValues: new Dictionary<string, string>
            {
                { nameof(categorySlug), categorySlug },
                { nameof(sectionSlug), sectionSlug },
                { nameof(chunkSlug), chunkSlug },
            }
        );

        return HandleNotifyShareResults(controller, notifySendResults, returnToModel);
    }

    private async Task<RecommendationsViewModel> BuildRecommendationsViewModel(
        QuestionnaireCategoryEntry category,
        QuestionnaireSectionEntry section,
        SubmissionRoutingDataModel submissionRoutingData,
        string sectionSlug,
        string categorySlug,
        int? currentRecommendationCount = null
    )
    {
        if (category.LandingPage is null)
        {
            throw new InvalidDataException(
                "Cannot build a recommendations model with an empty category slug"
            );
        }

        var establishmentId = await GetActiveEstablishmentIdOrThrowException();
        var contentfulReferences = section.CoreRecommendations.Select(cr => cr.Id);
        var details = await _recommendationService.GetLatestRecommendationStatusesAsync(
            establishmentId
        );

        var chunkViewModels = section
            .CoreRecommendations.Select(cr => new RecommendationChunkViewModel
            {
                Content = cr.Content,
                Header = cr.HeaderText,
                LastUpdated = details[cr.Id].DateCreated,
                Status = details[cr.Id].NewStatus ?? RecommendationStatus.NotStarted,
                Slug = cr.Slug,
            })
            .ToList();

        var submission = submissionRoutingData.Submission;

        return new RecommendationsViewModel
        {
            CategoryName = category.Header.Text,
            SectionName = section.Name,
            Chunks = chunkViewModels,
            LatestCompletionDate =
                submission?.DateCompleted.HasValue == true
                    ? DateTimeHelper.FormattedDateShort(submission.DateCompleted.Value)
                    : null,
            SectionSlug = sectionSlug,
            CategorySlug = categorySlug,
            CurrentChunkCount = currentRecommendationCount,
            SubmissionResponses = submission?.Responses ?? [],
        };
    }
}

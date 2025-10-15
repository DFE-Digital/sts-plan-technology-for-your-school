using Contentful.Core.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class RecommendationsViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IOptions<ContentfulOptions> contentfulOptions,
    IContentfulService contentfulService,
    ISubmissionService submissionService,
    IRecommendationService recommendationService,
    ICurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser), IRecommendationsViewBuilder
{
    private readonly ISubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly IRecommendationService _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
    private readonly ContentfulOptions _contentfulOptions = contentfulOptions.Value ?? throw new ArgumentNullException(nameof(contentfulOptions));

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
        var establishmentId = GetEstablishmentIdOrThrowException();
        var categoryHeaderText = await ContentfulService.GetCategoryHeaderTextBySlugAsync(categorySlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find category header text for slug {categorySlug}");
        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug, includeLevel: 2)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, section, isCompletedSubmission: true);

        var answerIds = submissionRoutingData.Submission!.Responses.Select(r => r.AnswerSysId);
        var recommendationChunks = section.CoreRecommendations.ToList();

        var currentRecommendationChunk = recommendationChunks.FirstOrDefault(chunk => chunk.SlugifiedLinkText == chunkSlug)
           ?? throw new ContentfulDataUnavailableException($"No recommendation chunk found with slug matching: {chunkSlug}");

        var currentRecommendationHistoryStatus = await _recommendationService.GetCurrentRecommendationStatusAsync(
            currentRecommendationChunk.Id,
            establishmentId
        );

        var currentRecommendationIndex = recommendationChunks.IndexOf(currentRecommendationChunk);
        var previousRecommendationChunk = currentRecommendationIndex > 0
                            ? recommendationChunks[currentRecommendationIndex - 1]
                            : null;
        var nextRecommendationChunk = currentRecommendationIndex != recommendationChunks.Count - 1
                            ? recommendationChunks[currentRecommendationIndex + 1]
                            : null;

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
            SelectedStatusKey = currentRecommendationHistoryStatus?.NewStatus ?? RecommendationStatus.NotStarted.GetDisplayName(),
            LastUpdated = currentRecommendationHistoryStatus?.DateCreated,
            SuccessMessageTitle = controller.TempData["StatusUpdateSuccessTitle"] as string,
            SuccessMessageBody = controller.TempData["StatusUpdateSuccessBody"] as string,
            StatusErrorMessage = controller.TempData["StatusUpdateError"] as string,
            StatusOptions = Enum.GetValues(typeof(RecommendationStatus))
                .Cast<RecommendationStatus>()
                .ToDictionary(
                    key => key.GetDisplayName(),
                    key => key.GetDisplayName()
                )
        };

        return controller.View(SingleRecommendationViewName, viewModel);
    }

    public async Task<IActionResult> RouteBySectionAndRecommendation(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        bool useChecklist
    )
    {
        var establishmentId = GetEstablishmentIdOrThrowException();
        var category = await ContentfulService.GetCategoryBySlugAsync(categorySlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find category for slug {categorySlug}");
        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, section, isCompletedSubmission: true);

        switch (submissionRoutingData.Status)
        {
            case SubmissionStatus.NotStarted:
                return controller.RedirectToHomePage();

            case SubmissionStatus.InProgress:
                return controller.RedirectToAction(
                    nameof(QuestionsController.GetQuestionBySlug),
                    nameof(QuestionsController).GetControllerNameSlug(),
                    new { categorySlug, sectionSlug, submissionRoutingData.NextQuestion!.Slug });

            case SubmissionStatus.CompleteNotReviewed:
                return controller.RedirectToCheckAnswers(categorySlug, sectionSlug);

            case SubmissionStatus.CompleteReviewed:
                var viewModel = BuildRecommendationsViewModel(
                    category,
                    submissionRoutingData,
                    section,
                    sectionSlug
                );

                var viewName = useChecklist
                    ? RecommendationsChecklistViewName
                    : RecommendationsViewName;

                return controller.View(viewName, viewModel);

            default:
                throw new InvalidOperationException($"Invalid journey status - {submissionRoutingData.Status}");
        }
    }

    private RecommendationsViewModel BuildRecommendationsViewModel(
        QuestionnaireCategoryEntry category,
        SubmissionRoutingDataModel submissionRoutingData,
        QuestionnaireSectionEntry section,
        string sectionSlug
    )
    {
        _ = GetEstablishmentIdOrThrowException();

        return new RecommendationsViewModel()
        {
            CategoryName = category.Header.Text,
            SectionName = section.Name,
            Chunks = section.CoreRecommendations.ToList(),
            LatestCompletionDate = submissionRoutingData.Submission!.DateCompleted.HasValue
                                ? DateTimeHelper.FormattedDateShort(submissionRoutingData.Submission!.DateCompleted.Value)
                                : null,
            SectionSlug = sectionSlug,
            SubmissionResponses = submissionRoutingData.Submission.Responses
        };
    }
}

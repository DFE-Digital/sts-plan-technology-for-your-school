using Contentful.Core.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class RecommendationsViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IOptions<ContentfulOptions> contentfulOptions,
    IContentfulService contentfulService,
    IRecommendationService recommendationService,
    ISubmissionService submissionService,
    ICurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser), IRecommendationsViewBuilder
{
    private readonly IRecommendationService _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
    private readonly ISubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly ContentfulOptions _contentfulOptions = contentfulOptions?.Value ?? throw new ArgumentNullException(nameof(contentfulOptions));

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

        var subtopicRecommendation = await ContentfulService.GetSubtopicRecommendationByIdAsync(section.Id, 3);
        if (subtopicRecommendation is null)
        {
            throw new ContentfulDataUnavailableException($"Could not find subtopic for section with ID '{section.Id}'");
        }

        var answerIds = submissionRoutingData.Submission!.Responses.Select(r => r.AnswerSysId);
        var subtopicChunks = subtopicRecommendation!.Section.GetRecommendationChunksByAnswerIds(answerIds);

        var currentChunk = subtopicChunks.FirstOrDefault(chunk => chunk.SlugifiedLinkText == chunkSlug)
           ?? throw new ContentfulDataUnavailableException($"No recommendation chunk found with slug matching: {chunkSlug}");

        var currentChunkIndex = subtopicChunks.IndexOf(currentChunk);
        var previousChunk = currentChunkIndex > 0
                            ? subtopicChunks[currentChunkIndex - 1]
                            : null;
        var nextChunk = currentChunkIndex != subtopicChunks.Count - 1
                            ? subtopicChunks[currentChunkIndex + 1]
                            : null;

        var viewModel = new SingleRecommendationViewModel
        {
            CategoryName = categoryHeaderText,
            CategorySlug = categorySlug,
            SectionSlug = sectionSlug,
            Section = section,
            Chunks = subtopicChunks,
            CurrentChunk = currentChunk,
            PreviousChunk = previousChunk,
            NextChunk = nextChunk,
            CurrentChunkPosition = currentChunkIndex + 1,
            TotalChunks = subtopicChunks.Count
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
                return controller.RedirectToCheckAnswers(categorySlug, sectionSlug, null);

            case SubmissionStatus.CompleteReviewed:
                var viewModel = await BuildRecommendationsViewModel(
                    category,
                    submissionRoutingData,
                    section.Id,
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

    public async Task<IActionResult> RouteBySectionSlugAndMaturity(Controller controller, string sectionSlug, string? maturity)
    {
        if (!_contentfulOptions.UsePreviewApi)
        {
            return controller.RedirectToHomePage();
        }

        var establishmentId = GetEstablishmentIdOrThrowException();
        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, section, isCompletedSubmission: false);

        var subtopicRecommendation = await ContentfulService.GetSubtopicRecommendationByIdAsync(submissionRoutingData.QuestionnaireSection.Id)
            ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for: {submissionRoutingData.QuestionnaireSection.Name}");

        var intro = subtopicRecommendation.Intros
            .Find(intro => string.Equals(intro.Maturity, maturity, StringComparison.InvariantCultureIgnoreCase))
                ?? subtopicRecommendation.Intros[0];

        var viewModel = new RecommendationsViewModel()
        {
            SectionName = submissionRoutingData.QuestionnaireSection.Name,
            //Intro = intro,
            Chunks = subtopicRecommendation.Section.Chunks.ToList(),
            Slug = "preview",
        };

        return controller.View(RecommendationsViewName, viewModel);
    }

    private async Task<RecommendationsViewModel> BuildRecommendationsViewModel(
        QuestionnaireCategoryEntry category,
        SubmissionRoutingDataModel submissionRoutingData,
        string sectionId,
        string sectionSlug
    )
    {
        var establishmentId = GetEstablishmentIdOrThrowException();
        var subtopicRecommendation = await ContentfulService.GetSubtopicRecommendationByIdAsync(sectionId);
        if (subtopicRecommendation is null)
        {
            throw new ContentfulDataUnavailableException($"Could not find subtopic for section with ID '{sectionId}'");
        }

        var subtopicIntro = subtopicRecommendation?.GetRecommendationByMaturity(submissionRoutingData.Maturity);
        if (subtopicIntro is null)
        {
            throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for section with ID '{sectionId}' and maturity '{submissionRoutingData.Maturity}");
        }

        var answerIds = submissionRoutingData.Submission!.Responses.Select(r => r.AnswerSysId);
        var subtopicChunks = subtopicRecommendation!.Section.GetRecommendationChunksByAnswerIds(answerIds);

        return new RecommendationsViewModel()
        {
            CategoryName = category.Header.Text,
            SectionName = subtopicRecommendation.Subtopic.Name,
            Chunks = subtopicChunks,
            LatestCompletionDate = submissionRoutingData.Submission!.DateCompleted.HasValue
                                ? DateTimeHelper.FormattedDateShort(submissionRoutingData.Submission!.DateCompleted.Value)
                                : null,
            SectionSlug = sectionSlug,
            SubmissionResponses = submissionRoutingData.Submission.Responses
        };
    }
}

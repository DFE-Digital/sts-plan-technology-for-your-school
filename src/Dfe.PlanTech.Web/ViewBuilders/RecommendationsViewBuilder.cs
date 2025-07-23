using System.Threading;
using Contentful.Core.Configuration;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static System.Collections.Specialized.BitVector32;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class RecommendationsViewBuilder(
    ILogger<RecommendationsViewBuilder> logger,
    IOptions<ContentfulOptions> contentfulOptions,
    CurrentUser currentUser,
    ContentfulService contentfulService,
    RecommendationService recommendationService,
    SubmissionService submissionService
) : BaseViewBuilder(currentUser)
{
    private readonly ILogger<RecommendationsViewBuilder> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ContentfulOptions _contentfulOptions = contentfulOptions?.Value ?? throw new ArgumentNullException(nameof(contentfulOptions));
    private readonly ContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    private readonly RecommendationService _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
    private readonly SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    private const string RecommendationsChecklistViewName = "~/Views/Recommendations/RecommendationsChecklist.cshtml";
    private const string RecommendationsViewName = "~/Views/Recommendations/Recommendations.cshtml";

    public async Task<IActionResult> RouteBySectionAndRecommendation(
        Controller controller,
        string sectionSlug,
        string recommendationSlug,
        bool useChecklist
    )
    {
        var establishmentId = GetEstablishmentIdOrThrowException();
        var section = await _contentfulService.GetSectionBySlugAsync(sectionSlug);
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, sectionSlug);

        switch (submissionRoutingData.Status)
        {
            case SubmissionStatus.NotStarted:
                return controller.RedirectToHomePage();

            case SubmissionStatus.InProgress:
                return controller.RedirectToAction(
                    nameof(QuestionsController.GetQuestionBySlug),
                    nameof(QuestionsController),
                    new { sectionSlug, submissionRoutingData.NextQuestion!.Slug });

            case SubmissionStatus.CompleteNotReviewed:
                return controller.RedirectToCheckAnswers(sectionSlug);

            case SubmissionStatus.CompleteReviewed:
                if (!useChecklist)
                {
                    await _submissionService.SetLatestSubmissionViewedAsync(establishmentId, section.Id);
                }

                var viewModel = await BuildViewModel(
                    sectionSlug,
                    recommendationSlug,
                    section,
                    submissionRoutingData,
                    showYourSelfAssessmentChunk: !useChecklist
                );

                return controller.View(RecommendationsChecklistViewName, viewModel);

            default:
                throw new InvalidOperationException($"Invalid journey status - {submissionRoutingData.Status}");
        };
    }

    public async Task<IActionResult> RouteBySectionSlugAndMaturity(Controller controller, string sectionSlug, string? maturity)
    {
        if (!_contentfulOptions.UsePreviewApi)
        {
            return controller.RedirectToHomePage();
        }

        var establishmentId = GetEstablishmentIdOrThrowException();
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, sectionSlug);

        var subTopicRecommendation = await _contentfulService.GetSubTopicRecommendation(submissionRoutingData.QuestionnaireSection.Id)
            ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for: {submissionRoutingData.QuestionnaireSection.Name}");

        var intro = subTopicRecommendation.Intros
            .Find(intro => string.Equals(intro.Maturity, maturity, StringComparison.InvariantCultureIgnoreCase))
                ?? subTopicRecommendation.Intros[0];

        var viewModel = new RecommendationsViewModel()
        {
            SectionName = submissionRoutingData.QuestionnaireSection.Name,
            Intro = intro,
            Chunks = subTopicRecommendation.Section.Chunks,
            Slug = "preview",
        };

        return controller.View(RecommendationsViewName, viewModel);
    }

    public async Task<IActionResult> RouteFromSection(Controller controller, string sectionSlug)
    {
        var establishmentId = GetEstablishmentIdOrThrowException();
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, sectionSlug);
        var section = await _contentfulService.GetSectionBySlugAsync(sectionSlug);
        var subTopicRecommendation = await _contentfulService.GetSubTopicRecommendation(section.Id)
            ?? throw new ContentfulDataUnavailableException($"Could not find subtopic for section with ID '{section.Id}'");

        var subTopicIntro = subTopicRecommendation.GetRecommendationByMaturity(submissionRoutingData.Maturity)
            ?? throw new ContentfulDataUnavailableException($"Could not find recommendation intro subtopic ID '{subTopicRecommendation.Id}'");

        var recommendationSlug = subTopicIntro.Slug;
        return await RouteBySectionAndRecommendation(controller, sectionSlug, recommendationSlug, false);
    }

    private async Task<RecommendationsViewModel> BuildViewModel(
        string sectionSlug,
        string recommendationSlug,
        CmsQuestionnaireSectionDto section,
        SubmissionRoutingDataModel submissionRoutingData,
        bool showYourSelfAssessmentChunk
    )
    {
        var establishmentId = GetEstablishmentIdOrThrowException();
        var subTopicRecommendation = await _contentfulService.GetSubTopicRecommendation(section.Id);
        var subTopicIntro = subTopicRecommendation?.GetRecommendationByMaturity(submissionRoutingData.Maturity);

        if (subTopicRecommendation is null)
        {
            throw new ContentfulDataUnavailableException("Could not find subtopic for section with ID '{section.Id}'");
        }

        var answerIds = submissionRoutingData.Submission!.Responses.Select(r => r.AnswerSysId);
        var subTopicChunks = subTopicRecommendation.Section.GetRecommendationChunksByAnswerIds(answerIds);

        if (showYourSelfAssessmentChunk)
        {
            subTopicChunks.Add(new("Your self-assessment"));
        }

        return new RecommendationsViewModel()
        {
            SectionName = subTopicRecommendation.Subtopic.Name,
            Intro = subTopicIntro,
            Chunks = subTopicChunks,
            LatestCompletionDate = submissionRoutingData.Submission!.DateCompleted.HasValue
                                ? DateTimeHelper.FormattedDateShort(submissionRoutingData.Submission!.DateCompleted.Value)
                                : null,
            Slug = recommendationSlug,
            SectionSlug = sectionSlug,
            SubmissionResponses = submissionRoutingData.Submission.Responses
        };
    }
}

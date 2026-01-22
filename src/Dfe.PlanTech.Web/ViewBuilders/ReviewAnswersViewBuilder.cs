using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class ReviewAnswersViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    ISubmissionService submissionService,
    ICurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser), IReviewAnswersViewBuilder
{
    private readonly ISubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    public const string ViewAnswersViewName = "~/Views/ViewAnswers/ViewAnswers.cshtml";
    public const string CheckAnswersViewName = "~/Views/CheckAnswers/CheckAnswers.cshtml";
    public const string InlineRecommendationUnavailableErrorMessage = "Unable to save. Please try again. If this problem continues you can";

    public async Task<IActionResult> RouteToCheckAnswers(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string? errorMessage = null
    )
    {
        var establishmentId = await GetActiveEstablishmentIdOrThrowException();

        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");

        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(
            establishmentId,
            section,
            status: SubmissionStatus.InProgress
        );
        ReviewAnswersViewModel viewModel;

        switch (submissionRoutingData.Status)
        {
            case SubmissionStatus.NotStarted:
                return controller.RedirectToHomePage();

            case SubmissionStatus.InProgress:
            case SubmissionStatus.CompleteNotReviewed:
            case SubmissionStatus.CompleteReviewed:
                viewModel = await BuildCheckAnswersViewModel(submissionRoutingData, categorySlug, sectionSlug, errorMessage);
                return controller.View(CheckAnswersViewName, viewModel);

            default:
                return controller.RedirectToGetQuestionBySlug(categorySlug, sectionSlug, submissionRoutingData.NextQuestion!.Slug);
        }
    }

    public async Task<IActionResult> RouteToViewAnswers(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string? errorMessage = null
    )
    {
        var establishmentId = await GetActiveEstablishmentIdOrThrowException();
        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");

        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(
            establishmentId,
            section,
            status: SubmissionStatus.CompleteReviewed
        );

        ViewAnswersViewModel viewModel;
        switch (submissionRoutingData.Status)
        {
            case SubmissionStatus.NotStarted:
                return controller.RedirectToHomePage();

            case SubmissionStatus.InProgress:
            case SubmissionStatus.CompleteNotReviewed:
                return controller.RedirectToGetContinueSelfAssessment(categorySlug, sectionSlug);

            case SubmissionStatus.CompleteReviewed:
                if (submissionRoutingData.Submission is null)
                    throw new InvalidOperationException(
                        $"Submission cannot be null when status is {SubmissionStatus.CompleteReviewed}"
                    );

                viewModel = BuildViewAnswersViewModel(section, submissionRoutingData, categorySlug, sectionSlug);
                return controller.View(ViewAnswersViewName, viewModel);

            default:
                return controller.RedirectToGetQuestionBySlug(categorySlug, sectionSlug, submissionRoutingData.NextQuestion!.Slug);
        }
    }

    public async Task<IActionResult> ConfirmCheckAnswers(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string sectionName,
        int submissionId
    )
    {
        try
        {
            var establishmentId = await GetActiveEstablishmentIdOrThrowException();
            var userOrganisationId = CurrentUser.UserOrganisationId;
            var userId = GetUserIdOrThrowException();

            var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug);

            await _submissionService.ConfirmCheckAnswersAndUpdateRecommendationsAsync(
                establishmentId,
                CurrentUser.IsMat ? userOrganisationId : null,
                submissionId,
                userId,
                section
            );
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occurred while confirming a user's answers for submission {SubmissionId}", submissionId);
            controller.TempData["ErrorMessage"] = InlineRecommendationUnavailableErrorMessage;
            return controller.RedirectToCheckAnswers(categorySlug, sectionSlug);
        }

        controller.TempData["SectionName"] = sectionName;
        return controller.RedirectToCategoryLandingPage(categorySlug);
    }

    private static ViewAnswersViewModel BuildViewAnswersViewModel(
        QuestionnaireSectionEntry section,
        SubmissionRoutingDataModel submissionModel,
        string categorySlug,
        string sectionSlug
    )
    {
        var submissionResponses = submissionModel.Submission?.Responses ?? [];

        // Get ordered CORE responses from section questions and select out the responses from the submission
        var orderedCoreResponses = section.Questions
            .Select(q => submissionModel.Submission?.Responses?.FirstOrDefault(r =>
                q.Sys is not null && r.QuestionSysId == q.Sys.Id))
            .ToList();

        // Get ordered Retired responses from section questions and select out the responses from the submission
        var orderedRetiredResponses = submissionResponses.OrderBy(r => r.Order).ToList();

        //Join the two lists together with core first so we only rely on the order columnn for the retired responses
        List<QuestionWithAnswerModel> responses = [.. orderedCoreResponses
            .Union(orderedRetiredResponses)
            .Where(r => r != null)
            .Cast<QuestionWithAnswerModel>()
        ];

        var viewModel = new ViewAnswersViewModel
        {
            AssessmentCompletedDate = submissionModel.Submission?.DateCompleted ?? DateTime.UtcNow,
            TopicName = section.Name,
            Responses = responses,
            CategorySlug = categorySlug,
            SectionSlug = sectionSlug
        };

        return viewModel;
    }

    private Task<ReviewAnswersViewModel> BuildCheckAnswersViewModel(
       SubmissionRoutingDataModel routingData,
       string categorySlug,
       string sectionSlug,
       string? errorMessage
   )
    {
        return BuildViewModel(routingData, categorySlug, sectionSlug, PageTitleConstants.CheckAnswers, UrlConstants.CheckAnswersSlug, errorMessage);
    }

    private async Task<ReviewAnswersViewModel> BuildViewModel(
        SubmissionRoutingDataModel routingData,
        string categorySlug,
        string sectionSlug,
        string pageTitle,
        string pageSlug,
        string? errorMessage
    )
    {
        List<ContentfulEntry> content = [];

        if (pageTitle.Equals(PageTitleConstants.CheckAnswers))
        {
            var page = await ContentfulService.GetPageBySlugAsync(UrlConstants.CheckAnswersSlug);
            content = page.Content ?? [];
        }

        var submissionResponsesViewModel = routingData.Submission is null
            ? null
            : new SubmissionResponsesViewModel(routingData.Submission);

        return new ReviewAnswersViewModel()
        {
            Title = new ComponentTitleEntry(pageTitle),
            Content = content,
            SectionName = routingData.QuestionnaireSection.Name,
            CategorySlug = categorySlug,
            SectionSlug = sectionSlug,
            Slug = pageSlug,
            SubmissionId = routingData.Submission?.SubmissionId,
            SubmissionResponses = submissionResponsesViewModel,
            ErrorMessage = errorMessage
        };
    }
}

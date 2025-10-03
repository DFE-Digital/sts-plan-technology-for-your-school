using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
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
        var establishmentId = GetEstablishmentIdOrThrowException();

        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");

        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(
            establishmentId,
            section,
            isCompletedSubmission: false
        );
        ReviewAnswersViewModel viewModel;

        switch (submissionRoutingData.Status)
        {
            case SubmissionStatus.NotStarted:
                return controller.RedirectToHomePage();

            case SubmissionStatus.InProgress:
            case SubmissionStatus.CompleteNotReviewed:
            case SubmissionStatus.CompleteReviewed:
                viewModel = await BuildCheckAnswersViewModel(controller, submissionRoutingData, categorySlug, sectionSlug, errorMessage);
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
        var establishmentId = GetEstablishmentIdOrThrowException();
        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");

        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(
            establishmentId,
            section,
            isCompletedSubmission: true
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

                viewModel = BuildViewAnswersViewModel(controller, section, submissionRoutingData, categorySlug, sectionSlug, errorMessage);
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
            await _submissionService.ConfirmCheckAnswersAsync(submissionId);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "There was an error while trying to calculate the maturity of submission {SubmissionId}", submissionId);
            controller.TempData["ErrorMessage"] = InlineRecommendationUnavailableErrorMessage;
            return controller.RedirectToCheckAnswers(categorySlug, sectionSlug);
        }

        controller.TempData["SectionName"] = sectionName;
        return controller.RedirectToCategoryLandingPage(categorySlug);
    }

    private ViewAnswersViewModel BuildViewAnswersViewModel(
        Controller controller,
        QuestionnaireSectionEntry section,
        SubmissionRoutingDataModel submissionModel,
        string categorySlug,
        string sectionSlug,
        string? errorMessage
    )
    {
        var viewModel = new ViewAnswersViewModel
        {
            TrustName = submissionModel.Submission?.Establishment.OrgName ?? "a school", //not sure on the appropriate fallback here
            AssessmentCompletedDate = submissionModel.Submission?.DateCompleted ?? DateTime.UtcNow,
            TopicName = section.Name,
            Responses = submissionModel.Submission?.Responses,
            CategorySlug = categorySlug,
            SectionSlug = sectionSlug
        };

        return viewModel;
    }

    private Task<ReviewAnswersViewModel> BuildCheckAnswersViewModel(
       Controller controller,
       SubmissionRoutingDataModel routingData,
       string categorySlug,
       string sectionSlug,
       string? errorMessage
   )
    {
        return BuildViewModel(controller, routingData, categorySlug, sectionSlug, PageTitleConstants.CheckAnswers, UrlConstants.CheckAnswersSlug, errorMessage);
    }

    private async Task<ReviewAnswersViewModel> BuildViewModel(
        Controller controller,
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

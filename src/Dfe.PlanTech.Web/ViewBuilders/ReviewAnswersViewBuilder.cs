using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
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
    IRecommendationService recommendationService,
    ISubmissionService submissionService,
    ICurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser), IReviewAnswersViewBuilder
{
    private readonly ISubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly IRecommendationService _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));

    public const string ChangeAnswersViewName = "~/Views/ChangeAnswers/ChangeAnswers.cshtml";
    public const string CheckAnswersViewName = "~/Views/CheckAnswers/CheckAnswers.cshtml";
    public const string InlineRecommendationUnavailableErrorMessage = "Unable to save. Please try again. If this problem continues you can";

    public async Task<IActionResult> RouteToCheckAnswers(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        bool? isChangeAnswersFlow,
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
            case SubmissionStatus.CompleteReviewed when isChangeAnswersFlow != false:
                viewModel = await BuildCheckAnswersViewModel(controller, submissionRoutingData, categorySlug, sectionSlug, errorMessage);
                return controller.View(CheckAnswersViewName, viewModel);

            case SubmissionStatus.CompleteReviewed when isChangeAnswersFlow == false:
                viewModel = await BuildChangeAnswersViewModel(controller, submissionRoutingData, categorySlug, sectionSlug, errorMessage);
                return controller.View(ChangeAnswersViewName, viewModel);

            default:
                return controller.RedirectToGetQuestionBySlug(categorySlug, sectionSlug, submissionRoutingData.NextQuestion!.Slug);
        }
    }

    public async Task<IActionResult> RouteToChangeAnswers(
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

        ReviewAnswersViewModel viewModel;
        switch (submissionRoutingData.Status)
        {
            case SubmissionStatus.NotStarted:
                return controller.RedirectToHomePage();

            case SubmissionStatus.InProgress:
                return controller.RedirectToGetNextUnansweredQuestion(categorySlug, sectionSlug);

            case SubmissionStatus.CompleteNotReviewed:
                viewModel = await BuildCheckAnswersViewModel(controller, submissionRoutingData, categorySlug, sectionSlug, errorMessage);
                return controller.View(CheckAnswersViewName, viewModel);

            case SubmissionStatus.CompleteReviewed:
                var newSubmission = await _submissionService.RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(establishmentId, submissionRoutingData.QuestionnaireSection);
                var responsesModel = new SubmissionResponsesModel(newSubmission, section);
                submissionRoutingData = new SubmissionRoutingDataModel
                (
                    nextQuestion: section.Questions.First(),
                    questionnaireSection: section,
                    submission: responsesModel,
                    status: newSubmission!.Status!.ToSubmissionStatus()
                );
                viewModel = await BuildChangeAnswersViewModel(controller, submissionRoutingData, categorySlug, sectionSlug, errorMessage);
                return controller.View(ChangeAnswersViewName, viewModel);

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
            var establishmentId = GetEstablishmentIdOrThrowException();
            var matEstablishmentId = CurrentUser.MatEstablishmentId;
            var userId = GetUserIdOrThrowException();

            var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug);

            _submissionService.ConfirmCheckAnswersAndUpdateRecommendationsAsync(
                establishmentId,
                matEstablishmentId,
                userId,
                section
            );
        }
        catch (Exception e)
        {
            Logger.LogError(e, "There was an error while trying to calculate the maturity of submission {SubmissionId}", submissionId);
            controller.TempData["ErrorMessage"] = InlineRecommendationUnavailableErrorMessage;
            return controller.RedirectToCheckAnswers(categorySlug, sectionSlug, false);
        }

        controller.TempData["SectionName"] = sectionName;
        return controller.RedirectToCategoryLandingPage(categorySlug);
    }

    private Task<ReviewAnswersViewModel> BuildChangeAnswersViewModel(
        Controller controller,
        SubmissionRoutingDataModel routingData,
        string categorySlug,
        string sectionSlug,
        string? errorMessage
    )
    {
        return BuildViewModel(controller, routingData, categorySlug, sectionSlug, PageTitleConstants.ChangeAnswers, UrlConstants.ChangeAnswersSlug, errorMessage);
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

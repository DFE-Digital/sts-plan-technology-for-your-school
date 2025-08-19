using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class ReviewAnswersViewBuilder(
    ILoggerFactory loggerFactory,
    CurrentUser currentUser,
    ContentfulService contentfulService,
    RecommendationService recommendationService,
    SubmissionService submissionService
) : BaseViewBuilder(loggerFactory, contentfulService, currentUser)
{
    private readonly ILogger<ReviewAnswersViewBuilder> _logger = loggerFactory.CreateLogger<ReviewAnswersViewBuilder>();
    private readonly RecommendationService _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
    private readonly SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

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
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(
            establishmentId,
            sectionSlug,
            isCompletedSubmission: isChangeAnswersFlow
        );
        ReviewAnswersViewModel viewModel;

        switch (submissionRoutingData.Status)
        {
            case SubmissionStatus.NotStarted:
                return controller.RedirectToHomePage();

            case SubmissionStatus.InProgress:
            case SubmissionStatus.CompleteNotReviewed:
                viewModel = await BuildCheckAnswersViewModel(controller, submissionRoutingData, categorySlug, sectionSlug, errorMessage);
                return controller.View(CheckAnswersViewName, viewModel);

            case SubmissionStatus.CompleteReviewed:
                if (isChangeAnswersFlow == false)
                {
                    await _submissionService.RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(establishmentId, submissionRoutingData.QuestionnaireSection);
                }
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
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(
            establishmentId,
            sectionSlug,
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
            await _submissionService.ConfirmCheckAnswersAsync(submissionId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "There was an error while trying to calculate the maturity of submission {SubmissionId}", submissionId);
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
        return BuildViewModel(controller, routingData, categorySlug, sectionSlug, PageTitleConstants.ChangeAnswers, RouteConstants.ChangeAnswersSlug, errorMessage);
    }

    private Task<ReviewAnswersViewModel> BuildCheckAnswersViewModel(
       Controller controller,
       SubmissionRoutingDataModel routingData,
       string categorySlug,
       string sectionSlug,
       string? errorMessage
   )
    {
        return BuildViewModel(controller, routingData, categorySlug, sectionSlug, PageTitleConstants.CheckAnswers, RouteConstants.CheckAnswersSlug, errorMessage);
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
        List<CmsEntryDto> content = [];

        if (pageTitle.Equals(PageTitleConstants.CheckAnswers))
        {
            var page = await ContentfulService.GetPageBySlugAsync(RouteConstants.CheckAnswersSlug);
            content = page.Content ?? [];
        }

        var submissionResponsesViewModel = routingData.Submission is null
            ? null
            : new SubmissionResponsesViewModel(routingData.Submission);

        return new ReviewAnswersViewModel()
        {
            Title = new CmsComponentTitleDto(pageTitle),
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

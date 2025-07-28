using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
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

    public const string InlineRecommendationUnavailableErrorMessage = "Unable to save. Please try again. If this problem continues you can";

    public async Task<IActionResult> RouteBasedOnSubmissionStatus(
        Controller controller,
        string sectionSlug,
        bool isChangeAnswersFlow,
        string? errorMessage = null
    )
    {
        var establishmentId = GetEstablishmentIdOrThrowException();
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, sectionSlug);
        var model = BuildChangeAnswersViewModel(controller, submissionRoutingData, sectionSlug, errorMessage);

        switch (submissionRoutingData.Status)
        {
            case SubmissionStatus.NotStarted:
                return controller.RedirectToHomePage();

            case SubmissionStatus.CompleteNotReviewed:
                return controller.View(ChangeAnswersController.ChangeAnswersViewName, model);

            case SubmissionStatus.CompleteReviewed:
                if (!isChangeAnswersFlow)
                {
                    await _submissionService.RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(establishmentId, submissionRoutingData.QuestionnaireSection.Sys.Id!);
                }
                return controller.View(ChangeAnswersController.ChangeAnswersViewName, model);

            default:
                return controller.RedirectToAction(nameof(QuestionsController.GetQuestionBySlug), nameof(QuestionsController), new { sectionSlug, submissionRoutingData.NextQuestion!.Slug });
        }
    }

    public async Task<IActionResult> RouteToSubtopicRecommendationIntroSlugAsync(Controller controller, string sectionSlug)
    {
        var establishmentId = GetEstablishmentIdOrThrowException();

        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug);

        var recommendationIntroSlug = _submissionService.GetSubmissionRoutingDataAsync(establishmentId, sectionSlug);

        return controller.RedirectToAction(
              RecommendationsController.GetRecommendationAction,
              RecommendationsController.ControllerName,
              new { sectionSlug, recommendationIntroSlug }
          );
    }

    public async Task<IActionResult> ConfirmCheckAnswers(
        Controller controller,
        string sectionSlug,
        string sectionName,
        int submissionId,
        string redirectOption
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
            return controller.RedirectToCheckAnswers(sectionSlug);
        }

        switch (redirectOption)
        {
            case RecommendationsController.GetRecommendationAction:
                var establishmentId = GetEstablishmentIdOrThrowException();
                var recommendationIntroSlug = await _recommendationService.GetRecommendationIntroSlug(establishmentId, sectionSlug);
                return controller.RedirectToRecommendation(sectionSlug, recommendationIntroSlug);
            case UrlConstants.HomePage:
                return controller.RedirectToHomePage();
            default:
                return controller.RedirectToCheckAnswers(sectionSlug);
        }
        ;
    }

    private async Task<ReviewAnswersViewModel> BuildChangeAnswersViewModel(
        Controller controller,
        SubmissionRoutingDataModel routingData,
        string sectionSlug,
        string? errorMessage
    )
    {
        List<CmsEntryDto> content = [];
        string pageTitle = string.Empty;
        string slug = string.Empty;

        if (controller is CheckAnswersController)
        {
            var page = await ContentfulService.GetPageBySlugAsync(CheckAnswersController.CheckAnswersPageSlug);
            content = page.Content;
            pageTitle = PageTitleConstants.CheckAnswers;
            slug = CheckAnswersController.CheckAnswersPageSlug;

        }
        else if (controller is ChangeAnswersController)
        {
            pageTitle = PageTitleConstants.ChangeAnswers;
            slug = ChangeAnswersController.ChangeAnswersPageSlug;
        }

        var submissionResponsesViewModel = routingData.Submission is null
            ? null
            : new SubmissionResponsesViewModel(routingData.Submission);

        return new ReviewAnswersViewModel()
        {
            Title = new CmsComponentTitleDto() { Text = pageTitle },
            Content = content,
            SectionName = routingData.QuestionnaireSection.Name,
            SectionSlug = sectionSlug,
            Slug = slug,
            SubmissionId = routingData.Submission?.SubmissionId,
            SubmissionResponses = submissionResponsesViewModel,
            ErrorMessage = errorMessage
        };
    }
}

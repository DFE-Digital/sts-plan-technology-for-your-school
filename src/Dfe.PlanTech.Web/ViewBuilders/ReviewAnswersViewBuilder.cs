using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class ReviewAnswersViewBuilder(
    ILogger<ReviewAnswersViewBuilder> logger,
    CurrentUser currentUser,
    ContentfulService contentfulService,
    SubmissionService submissionService
) : BaseViewBuilder(currentUser)
{
    private readonly ILogger<ReviewAnswersViewBuilder> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
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
                return controller.RedirectToSelfAssessment();

            case SubmissionStatus.CompleteNotReviewed:
                return controller.View(ChangeAnswersController.ChangeAnswersViewName, model);

            case SubmissionStatus.CompleteReviewed:
                if (!isChangeAnswersFlow)
                {
                    await _submissionService.RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(establishmentId, submissionRoutingData.QuestionnaireSection.Sys.Id!);
                }
                return controller.View(ChangeAnswersController.ChangeAnswersViewName, model);

            default:
                return QuestionsController.RedirectToQuestionBySlug(sectionSlug, submissionRoutingData.NextQuestion!.Slug, controller);
        }
    }

    public IActionResult RouteToSubtopicRecommendationIntroSlugAsync(Controller controller, string sectionSlug)
    {
        var establishmentId = GetEstablishmentIdOrThrowException();

        var recommendationIntroSlug = _submissionService.GetRecommendationIntroSlug(establishmentId, sectionSlug);

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
            await _submissionService.ConfirmCheckAnswers(submissionId);
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
                var recommendationIntroSlug = await _submissionService.GetRecommendationIntroSlug(establishmentId, sectionSlug);
                return controller.RedirectToRecommendation(sectionSlug, recommendationIntroSlug);
            case UrlConstants.SelfAssessmentPage:
                return controller.RedirectToSelfAssessment();
            default:
                return controller.RedirectToCheckAnswers(sectionSlug);
        };
    }

    private async Task<ReviewAnswersViewModel> BuildChangeAnswersViewModel(
        Controller controller,
        SubmissionRoutingDataModel routingData,
        string sectionSlug,
        string? errorMessage
    )
    {
        List<CmsEntryDto>? content = null;
        string pageTitle = string.Empty;
        string slug = string.Empty;

        if (controller is CheckAnswersController)
        {
            var page = await _contentfulService.GetPageBySlugAsync(CheckAnswersController.CheckAnswersPageSlug);
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
            SubmissionId = routingData.Submission?.Id,
            SubmissionResponses = submissionResponsesViewModel,
            ErrorMessage = errorMessage
        };
    }
}

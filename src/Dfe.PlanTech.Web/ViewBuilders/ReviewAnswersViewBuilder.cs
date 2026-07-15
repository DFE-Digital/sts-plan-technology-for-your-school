using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class ReviewAnswersViewBuilder(
    ILogger<ReviewAnswersViewBuilder> logger,
    IContentfulService contentfulService,
    ICurrentUserProvider currentUser,
    ISubmissionService submissionService,
    IMatEstablishmentProvider matEstablishmentProvider
) : BaseViewBuilder(logger, contentfulService, currentUser), IReviewAnswersViewBuilder
{
    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    private readonly IMatEstablishmentProvider _matEstablishmentProvider =
        matEstablishmentProvider ?? throw new ArgumentNullException(nameof(matEstablishmentProvider));

    public const string ViewAnswersViewName = "~/Views/ViewAnswers/ViewAnswers.cshtml";
    public const string CheckAnswersViewName = "~/Views/CheckAnswers/CheckAnswers.cshtml";
    public const string InlineRecommendationUnavailableErrorMessage =
        "Unable to save. Please try again. If this problem continues you can";

    public async Task<IActionResult> RouteToCheckAnswers(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string? errorMessage = null
    )
    {
        var establishmentId = await GetRoutingEstablishmentId();

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );

        var submissionRoutingData =
            await _submissionService.GetSubmissionRoutingDataAsync(
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
                viewModel = await BuildCheckAnswersViewModel(
                    submissionRoutingData,
                    categorySlug,
                    sectionSlug,
                    errorMessage
                );

                return controller.View(
                    CheckAnswersViewName,
                    viewModel
                );

            default:
                return controller.RedirectToGetQuestionBySlug(
                    categorySlug,
                    sectionSlug,
                    submissionRoutingData.NextQuestion!.Slug
                );
        }
    }

    public async Task<IActionResult> RouteToViewAnswers(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string? errorMessage = null
    )
    {
        var establishmentId = await GetRoutingEstablishmentId();

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );

        var submissionRoutingData =
            await _submissionService.GetSubmissionRoutingDataAsync(
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
                return controller.RedirectToGetContinueSelfAssessment(
                    categorySlug,
                    sectionSlug
                );

            case SubmissionStatus.CompleteReviewed:
                if (submissionRoutingData.Submission is null)
                {
                    throw new InvalidOperationException(
                        $"Submission cannot be null when status is {SubmissionStatus.CompleteReviewed}"
                    );
                }

                viewModel = BuildViewAnswersViewModel(
                    section,
                    submissionRoutingData,
                    categorySlug,
                    sectionSlug
                );

                return controller.View(
                    ViewAnswersViewName,
                    viewModel
                );

            default:
                return controller.RedirectToGetQuestionBySlug(
                    categorySlug,
                    sectionSlug,
                    submissionRoutingData.NextQuestion!.Slug
                );
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
            var establishmentId =
                await GetActiveEstablishmentIdOrThrowException();

            var userOrganisationId =
                CurrentUser.UserOrganisationId;

            var userId =
                GetUserIdOrThrowException();

            var section =
                await ContentfulService.GetSectionBySlugAsync(sectionSlug)
                ?? throw new ContentfulDataUnavailableException(
                    $"Could not find section for slug {sectionSlug}"
                );

            if (CurrentUser.IsMat)
            {
                var selectedEstablishmentIds =
                    _matEstablishmentProvider
                        .GetSelectedEstablishmentIdsFromSession()
                        .ToArray();

                if (selectedEstablishmentIds.Length > 0)
                {
                    foreach (
                        var selectedEstablishmentId
                        in selectedEstablishmentIds
                    )
                    {
                        var submissionModel =
                            await _submissionService
                                .GetLatestSubmissionResponsesModel(
                                    selectedEstablishmentId,
                                    section,
                                    SubmissionStatus.InProgress
                                );

                        if (submissionModel is null)
                        {
                            throw new InvalidOperationException(
                                $"Could not find an in-progress submission for establishment {selectedEstablishmentId}"
                            );
                        }

                        await _submissionService
                            .ConfirmCheckAnswersAndUpdateRecommendationsAsync(
                                selectedEstablishmentId,
                                userOrganisationId,
                                submissionModel.SubmissionId,
                                userId,
                                section
                            );
                    }
                }
                else
                {
                    await _submissionService
                        .ConfirmCheckAnswersAndUpdateRecommendationsAsync(
                            establishmentId,
                            userOrganisationId,
                            submissionId,
                            userId,
                            section
                        );
                }
            }
            else
            {
                await _submissionService
                    .ConfirmCheckAnswersAndUpdateRecommendationsAsync(
                        establishmentId,
                        null,
                        submissionId,
                        userId,
                        section
                    );
            }
        }
        catch (Exception exception)
        {
            Logger.LogError(
                exception,
                "An error occurred while confirming a user's answers for submission {SubmissionId}",
                submissionId
            );

            controller.TempData["ErrorMessage"] =
                InlineRecommendationUnavailableErrorMessage;

            return controller.RedirectToCheckAnswers(
                categorySlug,
                sectionSlug
            );
        }

        controller.TempData["SectionName"] = sectionName;

        if (CurrentUser.IsMat)
        {
            return controller.RedirectToTrustSelfAssessmentSummary(
                categorySlug,
                sectionSlug
            );
        }

        return controller.RedirectToSchoolSelfAssessmentSummary(
            categorySlug,
            sectionSlug
        );
    }

    public static ViewAnswersViewModel BuildViewAnswersViewModel(
        QuestionnaireSectionEntry section,
        SubmissionRoutingDataModel submissionModel,
        string categorySlug,
        string sectionSlug,
        bool isMatInProgressView = false,
        string? schoolName = null
    )
    {
        var orderedCoreResponses = section
            .Questions
            .Where(question => question.Sys is not null)
            .Select(question =>
                submissionModel.Submission?.Responses?.FirstOrDefault(
                    response =>
                        response.QuestionSysId == question.Sys!.Id
                )
            )
            .ToList();

        var orderedRetiredResponses =
            submissionModel.Submission?.Responses?
                .OrderBy(response => response.Order)
                .ToList()
            ?? [];

        List<QuestionWithAnswerModel> responses =
        [
            .. orderedCoreResponses
                .Union(orderedRetiredResponses)
                .Where(response => response is not null)
                .Cast<QuestionWithAnswerModel>(),
        ];

        return new ViewAnswersViewModel
        {
            AssessmentCompletedDate =
                submissionModel.Submission?.DateCompleted
                ?? DateTime.UtcNow,
            TopicName = section.Name,
            Responses = responses,
            CategorySlug = categorySlug,
            SectionSlug = sectionSlug,
            IsMatInProgressView = isMatInProgressView,
            SchoolName = schoolName,
            StartedBySchoolName = schoolName,
            DateStarted = submissionModel.Submission?.DateCreated,
            QuestionsAnswered = responses.Count,
            TotalQuestions = section.Questions.Count(),
            ShowInProgressDisclaimer = isMatInProgressView,
            BackLinkHref = isMatInProgressView
                ? $"/school/{categorySlug}"
                : $"/{categorySlug}",
            BackButtonText = isMatInProgressView
                ? $"Back to {section.Name.ToLower()}"
                : "Back to recommendations",
        };
    }

    private async Task<ReviewAnswersViewModel> BuildCheckAnswersViewModel(
        SubmissionRoutingDataModel routingData,
        string categorySlug,
        string sectionSlug,
        string? errorMessage
    )
    {
        var matEstablishmentModel =
            await _matEstablishmentProvider.PopulateMatSelectedSchools(
                CurrentUser
            );

        return await BuildViewModel(
            routingData,
            categorySlug,
            sectionSlug,
            PageTitleConstants.CheckAnswers,
            UrlConstants.CheckAnswersSlug,
            errorMessage,
            matEstablishmentModel
        );
    }

    private async Task<int> GetRoutingEstablishmentId()
    {
        var activeEstablishmentId =
            await GetActiveEstablishmentIdOrThrowException();

        if (!CurrentUser.IsMat)
        {
            return activeEstablishmentId;
        }

        var selectedEstablishmentId =
            _matEstablishmentProvider
                .GetSelectedEstablishmentIdsFromSession()
                .FirstOrDefault();

        return selectedEstablishmentId > 0
            ? selectedEstablishmentId
            : activeEstablishmentId;
    }

    private async Task<ReviewAnswersViewModel> BuildViewModel(
        SubmissionRoutingDataModel routingData,
        string categorySlug,
        string sectionSlug,
        string pageTitle,
        string pageSlug,
        string? errorMessage,
        MatEstablishmentModel? matEstablishmentModel = null
    )
    {
        List<ContentfulEntry> content = [];

        if (pageTitle.Equals(PageTitleConstants.CheckAnswers))
        {
            var page =
                await ContentfulService.GetPageBySlugAsync(
                    UrlConstants.CheckAnswersSlug
                );

            content = page.Content ?? [];
        }

        var submissionResponsesViewModel =
            routingData.Submission is null
                ? null
                : new SubmissionResponsesViewModel(
                    routingData.Submission
                );

        return new ReviewAnswersViewModel
        {
            Title = new ComponentTitleEntry(pageTitle),
            Content = content,
            SectionName = routingData.QuestionnaireSection.Name,
            CategorySlug = categorySlug,
            SectionSlug = sectionSlug,
            Slug = pageSlug,
            SubmissionId = routingData.Submission?.SubmissionId,
            SubmissionResponses = submissionResponsesViewModel,
            ErrorMessage = errorMessage,
            MatEstablishmentModel = matEstablishmentModel,
        };
    }
}
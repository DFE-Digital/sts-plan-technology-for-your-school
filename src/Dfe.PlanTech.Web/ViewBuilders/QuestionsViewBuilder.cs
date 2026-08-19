using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class QuestionsViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    ICurrentUserProvider currentUser,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IOptions<ErrorMessagesConfiguration> errorMessages,
    ContentfulOptionsConfiguration contentfulOptions,
    IQuestionService questionService,
    ISubmissionService submissionService,
    IEstablishmentService establishmentService,
    IMatEstablishmentProvider matEstablishmentProvider
) : BaseViewBuilder(logger, contentfulService, currentUser), IQuestionsViewBuilder
{
    private readonly IQuestionService _questionService =
        questionService ?? throw new ArgumentNullException(nameof(questionService));
    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly IEstablishmentService establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly ContactOptionsConfiguration _contactOptions =
        contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
    private readonly ErrorMessagesConfiguration _errorMessages =
        errorMessages?.Value ?? throw new ArgumentNullException(nameof(errorMessages));
    private readonly ContentfulOptionsConfiguration _contentfulOptions =
        contentfulOptions ?? throw new ArgumentNullException(nameof(contentfulOptions));
    private readonly IMatEstablishmentProvider _matEstablishmentProvider =
        matEstablishmentProvider
        ?? throw new ArgumentNullException(nameof(matEstablishmentProvider));

    private const string QuestionView = "Question";
    private const string InterstitialPagePath = "~/Views/Pages/Page.cshtml";
    private const string ContinueSelfAssessmentView = "ContinueSelfAssessment";
    private const string RestartObsoleteAssessmentView = "RestartObsoleteAssessment";

    public async Task<IActionResult> RouteBySlugAndQuestionAsync(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        string questionSlug,
        string? returnTo
    )
    {
        var establishmentId = await GetRoutingEstablishmentId();

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );

        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(
            establishmentId,
            section,
            status: SubmissionStatus.InProgress
        );

        var isSlugForNextQuestion =
            submissionRoutingData.NextQuestion?.Slug?.Equals(questionSlug) ?? false;

        if (isSlugForNextQuestion)
        {
            var nextQuestionViewModel = await GenerateViewModel(
                controller,
                submissionRoutingData.NextQuestion!,
                submissionRoutingData.QuestionnaireSection,
                categorySlug,
                sectionSlug,
                null,
                returnTo
            );

            return controller.View(QuestionView, nextQuestionViewModel);
        }

        if (submissionRoutingData.Status.Equals(SubmissionStatus.NotStarted))
        {
            return controller.RedirectToInterstitialPage(sectionSlug);
        }

        if (submissionRoutingData.Submission?.Responses is null)
        {
            throw new InvalidOperationException(
                $"No responses were found for section '{submissionRoutingData.QuestionnaireSection.Id}'"
            );
        }

        var question = submissionRoutingData.GetQuestionForSlug(questionSlug);

        var isQuestionInResponses = submissionRoutingData.IsQuestionInResponses(question.Id);

        if (isQuestionInResponses)
        {
            var latestResponseForQuestion = submissionRoutingData.GetLatestResponseForQuestion(
                question.Id
            );

            var viewModel = await GenerateViewModel(
                controller,
                question,
                submissionRoutingData.QuestionnaireSection,
                categorySlug,
                sectionSlug,
                latestResponseForQuestion.AnswerSysId,
                returnTo
            );

            return controller.View(QuestionView, viewModel);
        }

        if (submissionRoutingData.Status.Equals(SubmissionStatus.InProgress))
        {
            var nextQuestionSlug =
                submissionRoutingData.NextQuestion?.Slug
                ?? throw new InvalidDataException("NextQuestion is null");

            return await RouteBySlugAndQuestionAsync(
                controller,
                categorySlug,
                sectionSlug,
                nextQuestionSlug,
                returnTo
            );
        }

        return controller.RedirectToCheckAnswers(categorySlug, sectionSlug);
    }

    public async Task<IActionResult> RouteToInterstitialPage(
        Controller controller,
        string categorySlug,
        string sectionSlug
    )
    {
        var interstitialPage =
            await ContentfulService.GetPageBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find interstitial page for section {sectionSlug}"
            );

        var isMatWithoutSelectedSchool =
            CurrentUser.IsMat && string.IsNullOrWhiteSpace(CurrentUser.GroupSelectedSchoolUrn);

        if (isMatWithoutSelectedSchool)
        {
            interstitialPage.Content = interstitialPage
                .Content?.Where(x => x is not ComponentButtonWithEntryReferenceEntry)
                .ToList();
        }

        var viewModel = new PageViewModel(interstitialPage)
        {
            ShowTrustSchoolAssessmentTable = isMatWithoutSelectedSchool,
        };

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );

        if (isMatWithoutSelectedSchool)
        {
            viewModel.TrustSchoolAssessments = await BuildTrustSchoolAssessments(
                categorySlug,
                sectionSlug,
                section
            );

            viewModel.TrustSchoolAssessmentContinueHref =
                $"/groups/{categorySlug}/{sectionSlug}/self-assessment/{UrlConstants.GroupsSelectSchoolsToAssessSlug}";
        }

        return controller.View(InterstitialPagePath, viewModel);
    }

    public async Task<IActionResult> RouteByQuestionId(Controller controller, string questionId)
    {
        if (!_contentfulOptions.UsePreviewApi)
            return controller.Redirect(UrlConstants.HomePage);

        var question = await ContentfulService.GetQuestionByIdAsync(questionId);

        var viewModel = await GenerateViewModel(controller, question, null, null, null, null, null);
        return controller.View(QuestionView, viewModel);
    }

    public async Task<IActionResult> RouteToNextUnansweredQuestion(
        Controller controller,
        string categorySlug,
        string sectionSlug
    )
    {
        var establishmentId = await GetRoutingEstablishmentId();
        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug);

        try
        {
            var nextQuestion = await _questionService.GetNextUnansweredQuestion(
                establishmentId,
                section
            );
            if (nextQuestion is null)
            {
                return controller.RedirectToCheckAnswers(categorySlug, sectionSlug);
            }

            return controller.RedirectToAction(
                nameof(QuestionsController.GetQuestionBySlug),
                new
                {
                    categorySlug,
                    sectionSlug,
                    questionSlug = nextQuestion.Slug,
                }
            );
        }
        catch (DatabaseException)
        {
            // Remove the current invalid submission and redirect to self-assessment page
            await _submissionService.SetSubmissionInaccessibleAsync(establishmentId, section.Id);

            controller.TempData["SubtopicError"] = await BuildErrorMessage();
            return controller.RedirectToAction(
                nameof(PagesController.GetByRoute),
                nameof(PagesController).GetControllerNameSlug(),
                new { route = UrlConstants.Home }
            );
        }
    }

    private async Task<List<TrustSchoolAssessmentRowViewModel>> BuildTrustSchoolAssessments(
        string categorySlug,
        string sectionSlug,
        QuestionnaireSectionEntry section
    )
    {
        var groupId =
            CurrentUser.UserOrganisationId
            ?? throw new InvalidDataException(
                "User is a MAT user but does not have an organisation ID"
            );

        var schools =
            await establishmentService.GetEstablishmentLinksWithRecommendationCounts(groupId) ?? [];

        var rows = new List<TrustSchoolAssessmentRowViewModel>();

        foreach (var school in schools)
        {
            var schoolEstablishment = await establishmentService.GetEstablishmentByReferenceAsync(
                school.Urn
            );

            var submission = schoolEstablishment is null
                ? null
                : await _submissionService.GetLatestSubmissionResponsesModel(
                    schoolEstablishment.Id,
                    section,
                    [
                        SubmissionStatus.InProgress,
                        SubmissionStatus.CompleteNotReviewed,
                        SubmissionStatus.CompleteReviewed,
                    ]
                );

            var status = submission?.Status ?? SubmissionStatus.NotStarted;

            if (status == SubmissionStatus.CompleteReviewed)
            {
                continue;
            }

            var hasInProgressSubmission =
                status is SubmissionStatus.InProgress or SubmissionStatus.CompleteNotReviewed;

            rows.Add(
                new TrustSchoolAssessmentRowViewModel
                {
                    SchoolName = school.EstablishmentName,
                    Status = status,
                    ViewAnswersHref = hasInProgressSubmission
                        ? $"/school/{categorySlug}/{sectionSlug}/self-assessment/view-answers?schoolUrn={school.Urn}"
                        : null,
                }
            );
        }

        return rows;
    }

    public async Task<IActionResult> RouteToContinueSelfAssessmentPage(
        Controller controller,
        string categorySlug,
        string sectionSlug
    )
    {
        var establishmentId = await GetActiveEstablishmentIdOrThrowException();
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );

        var submissionModel = await _submissionService.GetLatestSubmissionResponsesModel(
            establishmentId,
            section,
            status: null
        );

        if (submissionModel is null || !submissionModel.HasResponses)
        {
            return controller.RedirectToInterstitialPage(sectionSlug);
        }

        if (submissionModel.Status == SubmissionStatus.Obsolete)
        {
            var restartObsoleteViewModel = new RestartObsoleteAssessmentViewModel
            {
                TopicName = section.Name,
                CategorySlug = categorySlug,
                SectionSlug = sectionSlug,
            };

            return controller.View(RestartObsoleteAssessmentView, restartObsoleteViewModel);
        }
        ;

        var establishment =await establishmentService.GetEstablishmentByIdAsync(establishmentId);

        var viewModel = new ContinueSelfAssessmentViewModel
        {
            AssessmentStartDate = submissionModel.DateCreated ?? DateTime.UtcNow,
            AssessmentUpdatedDate = submissionModel.DateLastUpdated ?? DateTime.UtcNow,
            AnsweredCount = submissionModel.Responses.Count,
            QuestionsCount = section.Questions.Count(),
            TopicName = section.Name,
            Responses = submissionModel.Responses,
            CategorySlug = categorySlug,
            SectionSlug = sectionSlug,
            SchoolName = establishment?.OrgName ?? string.Empty,
        };

        return controller.View(ContinueSelfAssessmentView, viewModel);
    }

    public async Task<IActionResult> RestartSelfAssessment(
        Controller controller,
        string categorySlug,
        string sectionSlug,
        bool isObsoleteSubmissionFlow
    )
    {
        var establishmentId = await GetActiveEstablishmentIdOrThrowException();
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find interstitial page for section {sectionSlug}"
            );

        if (!isObsoleteSubmissionFlow)
        {
            await _submissionService.SetSubmissionInaccessibleAsync(establishmentId, section.Id);
        }
        ;

        return controller.RedirectToAction(
            nameof(QuestionsController.GetInterstitialPage),
            QuestionsController.Controller,
            new { categorySlug, sectionSlug }
        );
    }

    public async Task<IActionResult> ContinuePreviousAssessment(
        Controller controller,
        string categorySlug,
        string sectionSlug
    )
    {
        var establishmentId = await GetActiveEstablishmentIdOrThrowException();
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find interstitial page for section {sectionSlug}"
            );

        await _submissionService.RestoreInaccessibleSubmissionAsync(establishmentId, section.Id);

        return controller.RedirectToAction(
            nameof(QuestionsController.GetNextUnansweredQuestion),
            QuestionsController.Controller,
            new { categorySlug, sectionSlug }
        );
    }

    public async Task<IActionResult> SubmitAnswerAndRedirect(
        Controller controller,
        SubmitAnswerInputViewModel answerViewModel,
        string categorySlug,
        string sectionSlug,
        string questionSlug,
        string? returnTo
    )
    {
        var selectedEstablishmentIds = CurrentUser.IsMat
            ? _matEstablishmentProvider.GetSelectedEstablishmentIdsFromSession().ToArray()
            : [];

        var userId = GetUserIdOrThrowException();
        var activeEstablishmentId = await GetActiveEstablishmentIdOrThrowException();

        var userOrganisationId =
            CurrentUser.UserOrganisationId
            ?? throw new InvalidOperationException(
                "User organisation ID is null - user needs to be logged in to submit an answer"
            );

        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );

        var question = section.GetQuestionBySlug(questionSlug);

        if (!controller.ModelState.IsValid)
        {
            var viewModel = await GenerateViewModel(
                controller,
                question,
                section,
                categorySlug,
                sectionSlug,
                answerViewModel.ChosenAnswer?.Answer.Id,
                returnTo
            );

            viewModel.ErrorMessages = controller
                .ModelState.Values.SelectMany(value => value.Errors.Select(err => err.ErrorMessage))
                .ToArray();

            return controller.View(QuestionView, viewModel);
        }

        try
        {
            await SubmitAnswerForSelectedEstablishments(
                userId,
                userOrganisationId,
                answerViewModel,
                activeEstablishmentId,
                selectedEstablishmentIds
            );
        }
        catch (Exception e)
        {
            Logger.LogError(
                e,
                "An error occurred while submitting an answer with the following message: {Message}",
                e.Message
            );

            var viewModel = await GenerateViewModel(
                controller,
                question,
                section,
                categorySlug,
                sectionSlug,
                questionSlug,
                null
            );

            viewModel.ErrorMessages = ["Save failed. Please try again later."];

            return controller.View(QuestionView, viewModel);
        }

        var routingEstablishmentId =
            CurrentUser.IsMat && selectedEstablishmentIds.Length > 0
                ? selectedEstablishmentIds[0]
                : activeEstablishmentId;

        var targetEstablishmentIdsLog =
            CurrentUser.IsMat && selectedEstablishmentIds.Length > 0
                ? selectedEstablishmentIds
                : [activeEstablishmentId];

        Logger.LogInformation(
            "Submitting answer for establishment IDs: {TargetEstablishmentIds}",
            string.Join(", ", targetEstablishmentIdsLog)
        );

        var nextQuestion = await _questionService.GetNextUnansweredQuestion(
            routingEstablishmentId,
            section
        );

        if (nextQuestion is not null)
        {
            return controller.RedirectToAction(
                nameof(QuestionsController.GetQuestionBySlug),
                QuestionsController.Controller,
                new
                {
                    categorySlug,
                    sectionSlug,
                    questionSlug = nextQuestion.Slug,
                    returnTo,
                }
            );
        }

        return controller.RedirectToCheckAnswers(categorySlug, sectionSlug);
    }

    private async Task SubmitAnswerForSelectedEstablishments(
        int userId,
        int userOrganisationId,
        SubmitAnswerInputViewModel answerViewModel,
        int activeEstablishmentId,
        int[] selectedEstablishmentIds
    )
    {
        var establishmentIds =
            CurrentUser.IsMat && selectedEstablishmentIds.Length > 0
                ? selectedEstablishmentIds
                : [activeEstablishmentId];

        foreach (var establishmentId in establishmentIds)
        {
            await _submissionService.SubmitAnswerAsync(
                userId,
                establishmentId,
                userOrganisationId,
                answerViewModel.ToModel()
            );
        }
    }

    private async Task PopulateMatSelectedSchools(QuestionViewModel viewModel)
    {
        var selectedSchoolNames = await _matEstablishmentProvider.GetSelectedSchoolNamesAsync(
            CurrentUser
        );

        viewModel.IsMatBulkAssessment = _matEstablishmentProvider.IsBulkAssessment();

        viewModel.IsMatMultiSchoolAssessment = selectedSchoolNames.Count > 0;
        viewModel.SelectedSchoolCount = selectedSchoolNames.Count;
        viewModel.SelectedSchoolNames = selectedSchoolNames.ToList();
    }

    private async Task<int> GetRoutingEstablishmentId()
    {
        var activeEstablishmentId = await GetActiveEstablishmentIdOrThrowException();

        if (!CurrentUser.IsMat)
        {
            return activeEstablishmentId;
        }

        var selectedEstablishmentId = _matEstablishmentProvider
            .GetSelectedEstablishmentIdsFromSession()
            .FirstOrDefault();

        return selectedEstablishmentId > 0 ? selectedEstablishmentId : activeEstablishmentId;
    }

    private async Task<string> BuildErrorMessage()
    {
        var contactLink = await ContentfulService.GetLinkByIdAsync(_contactOptions.LinkId);
        var errorMessage = _errorMessages.ConcurrentUsersOrContentChange;

        if (!string.IsNullOrEmpty(contactLink?.Href))
        {
            errorMessage = errorMessage.Replace(
                "contact us",
                $"<a href=\"{contactLink.Href}\" target=\"_blank\">contact us</a>"
            );
        }

        return errorMessage;
    }

    private async Task<QuestionViewModel> GenerateViewModel(
        Controller controller,
        QuestionnaireQuestionEntry question,
        QuestionnaireSectionEntry? section,
        string? categorySlug,
        string? sectionSlug,
        string? latestAnswerContentfulId,
        string? returnTo
    )
    {
        controller.ViewData[StatePassingMechanismConstants.Title] = question.Text;

        if (!string.IsNullOrEmpty(returnTo))
        {
            controller.ViewData[StatePassingMechanismConstants.ReturnTo] = returnTo;
        }

        // Workaround, to avoid infinite loop due to bi-directional/circular references:
        foreach (var nextQuestion in question.Answers.Select(a => a.NextQuestion))
        {
            if (nextQuestion is null)
            {
                continue;
            }

            nextQuestion.Answers = [];
        }

        var viewModel = new QuestionViewModel
        {
            Question = question,
            AnswerSysId = latestAnswerContentfulId,
            SectionName = section?.Name,
            CategorySlug = categorySlug,
            SectionSlug = sectionSlug,
            SectionId = section?.Id,
        };

        await PopulateMatSelectedSchools(viewModel);

        return viewModel;
    }
}

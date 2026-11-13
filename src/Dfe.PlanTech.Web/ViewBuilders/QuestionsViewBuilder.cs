using Dfe.PlanTech.Application.Providers.Interfaces;
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
    ILogger<QuestionsViewBuilder> logger,
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
        var establishmentId = CurrentUser.GetFocusedSchool()!.EstablishmentId;

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
            CurrentUser.IsMat && CurrentUser.GetSelectedSchools().Count == 0;

        if (isMatWithoutSelectedSchool)
        {
            interstitialPage.Content = interstitialPage
                .Content?.Where(x => x is not ComponentButtonWithEntryReferenceEntry)
                .ToList();
        }

        var category =
            await ContentfulService.GetCategoryBySlugAsync(categorySlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find category for slug {categorySlug}"
            );

        var categoryName =
            category.Header?.Text
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find category header text for slug {categorySlug}"
            );

        var viewModel = new PageViewModel(interstitialPage)
        {
            ShowTrustSchoolAssessmentTable = isMatWithoutSelectedSchool,
            CategoryName = categoryName,
            CategorySlug = categorySlug,
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
                $"/trust/{categorySlug}/{sectionSlug}/self-assessment/{UrlConstants.GroupsSelectSchoolsToAssessSlug}";
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
        var establishmentId = CurrentUser.GetFocusedSchool()!.EstablishmentId;
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

            controller.TempData[StatePassingMechanismConstants.SubtopicError] =
                await BuildErrorMessage();
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
        var schools =
            await establishmentService.GetEstablishmentLinksWithRecommendationCounts(
                CurrentUser.Establishment.Id
            ) ?? [];

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
        var focusedEstablishmentId = CurrentUser.GetFocusedSchool().EstablishmentId;
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find section for slug {sectionSlug}"
            );

        var submissionModel = await _submissionService.GetLatestSubmissionResponsesModel(
            focusedEstablishmentId,
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

        var establishment = await establishmentService.GetEstablishmentByIdAsync(
            focusedEstablishmentId
        );

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
        var focusedEstablishmentId = CurrentUser.GetFocusedSchool().EstablishmentId;
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find interstitial page for section {sectionSlug}"
            );

        if (!isObsoleteSubmissionFlow)
        {
            await _submissionService.SetSubmissionInaccessibleAsync(
                focusedEstablishmentId,
                section.Id
            );
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
        var focusedEstablishmentId = CurrentUser.GetFocusedSchool().EstablishmentId;
        var section =
            await ContentfulService.GetSectionBySlugAsync(sectionSlug)
            ?? throw new ContentfulDataUnavailableException(
                $"Could not find interstitial page for section {sectionSlug}"
            );

        await _submissionService.RestoreInaccessibleSubmissionAsync(
            focusedEstablishmentId,
            section.Id
        );

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
        var selectedEstablishmentIds = CurrentUser.CanSelectSchools
            ? CurrentUser.GetSelectedSchoolEstablishmentIds()
            : [CurrentUser.GetFocusedSchool().EstablishmentId];

        if (selectedEstablishmentIds.Count == 0)
        {
            throw new InvalidOperationException(
                "User attempted to submit an answer but there are no focused or selected schools"
            );
        }

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

            viewModel.ErrorMessages = controller.ModelState.Values.SelectMany(value =>
                value.Errors.Select(err => err.ErrorMessage)
            );

            return controller.View(QuestionView, viewModel);
        }

        try
        {
            await SubmitAnswerForSelectedEstablishments(
                CurrentUser.UserId,
                CurrentUser.Establishment.Id,
                answerViewModel,
                selectedEstablishmentIds.ToArray()
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

        Logger.LogInformation(
            "Submitting answer for establishment IDs: {TargetEstablishmentIds}",
            string.Join(", ", selectedEstablishmentIds)
        );

        var nextQuestion = await _questionService.GetNextUnansweredQuestion(
            selectedEstablishmentIds.First(),
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
        int userEstablishmentId,
        SubmitAnswerInputViewModel answerViewModel,
        int[] selectedEstablishmentIds
    )
    {
        foreach (var establishmentId in selectedEstablishmentIds)
        {
            await _submissionService.SubmitAnswerAsync(
                userId,
                establishmentId,
                userEstablishmentId,
                answerViewModel.ToModel()
            );
        }
    }

    private void PopulateSelectedSchools(QuestionViewModel viewModel)
    {
        var selectedSchoolNames = CurrentUser.GetSelectedSchoolNames();

        viewModel.IsMatBulkAssessment = _matEstablishmentProvider.IsBulkAssessment();
        viewModel.SelectedSchoolCount = selectedSchoolNames.Count;
        viewModel.SelectedSchoolNames = selectedSchoolNames;
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

        if (CurrentUser.CanSelectSchools)
        {
            PopulateSelectedSchools(viewModel);
        }

        return viewModel;
    }
}

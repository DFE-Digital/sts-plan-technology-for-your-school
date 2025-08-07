using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class QuestionsViewBuilder(
    ILoggerFactory loggerFactory,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IOptions<ErrorMessagesConfiguration> errorMessages,
    IOptions<ErrorPagesConfiguration> errorPages,
    ContentfulOptionsConfiguration contentfulOptions,
    CurrentUser currentUser,
    ContentfulService contentfulService,
    QuestionService questionService,
    SubmissionService submissionService
) : BaseViewBuilder(loggerFactory, contentfulService, currentUser)
{
    private ILogger<QuestionsViewBuilder> _logger = loggerFactory.CreateLogger<QuestionsViewBuilder>();
    private ContactOptionsConfiguration _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
    private ErrorMessagesConfiguration _errorMessages = errorMessages?.Value ?? throw new ArgumentNullException(nameof(errorMessages));
    private ErrorPagesConfiguration _errorPages = errorPages?.Value ?? throw new ArgumentNullException(nameof(errorPages));
    private ContentfulOptionsConfiguration _contentfulOptions = contentfulOptions ?? throw new ArgumentNullException(nameof(contentfulOptions));
    private QuestionService _questionService = questionService ?? throw new ArgumentNullException(nameof(questionService));
    private SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    private const string QuestionView = "Question";


    public async Task<IActionResult> RouteBySlugAndQuestionAsync(Controller controller,
        string sectionSlug,
        string questionSlug,
        string? returnTo
    )
    {
        var establishmentId = GetEstablishmentIdOrThrowException();

        controller.TempData["ReturnTo"] = returnTo;

        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, sectionSlug);

        var isChangeAnswersFlow = returnTo == FlowConstants.ChangeAnswersFlow;
        var isSlugForNextQuestion = submissionRoutingData.NextQuestion?.Slug?.Equals(questionSlug) ?? false;

        if (isSlugForNextQuestion)
        {
            var nextQuestionViewModel = GenerateViewModel(
                controller,
                submissionRoutingData.NextQuestion!,
                submissionRoutingData.QuestionnaireSection,
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
                $"No responses were found for section '{submissionRoutingData.QuestionnaireSection.Id}'");
        }

        /*
         * Now check to see if the question is part of the latest user responses.
         * If so: 
         *   show page
         * If not:
         *   if on "check answers" status, redirect to check answers page 
         *   if on "next question" status, redirect to next question 
         */

        var question = submissionRoutingData.GetQuestionForSlug(questionSlug);
        var isQuestionInResponses = submissionRoutingData.IsQuestionInResponses(question.Id);

        if (isQuestionInResponses)
        {
            var latestResponseForQuestion = submissionRoutingData.GetLatestResponseForQuestion(question.Id);
            var viewModel = GenerateViewModel(
                controller,
                question,
                submissionRoutingData.QuestionnaireSection,
                sectionSlug,
                latestResponseForQuestion.AnswerSysId,
                null
            );

            return controller.View(QuestionView, viewModel);
        }

        if (submissionRoutingData.Status.Equals(SubmissionStatus.CompleteNotReviewed))
        {
            return controller.RedirectToCheckAnswers(sectionSlug);
        }

        if (submissionRoutingData.Status.Equals(SubmissionStatus.InProgress))
        {
            var nextQuestionSlug = submissionRoutingData.NextQuestion?.Slug
                ?? throw new InvalidDataException("NextQuestion is null");

            return await RouteBySlugAndQuestionAsync(controller, sectionSlug, nextQuestionSlug, returnTo);
        }

        throw new InvalidDataException("Should not be able to get here");
    }

    public async Task<IActionResult> RouteByQuestionId(Controller controller, string questionId)
    {
        if (!_contentfulOptions.UsePreviewApi)
            return controller.Redirect(UrlConstants.HomePage);

        var question = await ContentfulService.GetQuestionByIdAsync(questionId);

        var viewModel = GenerateViewModel(controller, question, null, null, null, null);
        return controller.View(QuestionView, viewModel);
    }

    public async Task<IActionResult> RouteToNextUnansweredQuestion(Controller controller, string sectionSlug)
    {
        var establishmentId = GetEstablishmentIdOrThrowException();
        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug);

        try
        {
            var nextQuestion = await _questionService.GetNextUnansweredQuestion(establishmentId, section);
            if (nextQuestion is null)
            {
                return controller.RedirectToCheckAnswers(sectionSlug);
            }

            return controller.RedirectToAction(nameof(QuestionsController.GetQuestionBySlug), new { sectionSlug, questionSlug = nextQuestion.Slug });
        }
        catch (DatabaseException)
        {
            // Remove the current invalid submission and redirect to self-assessment page
            await _submissionService.DeleteCurrentSubmissionSoftAsync(establishmentId, section.Id);

            controller.TempData["SubtopicError"] = await BuildErrorMessage();
            return controller.RedirectToAction(
                PagesController.GetPageByRouteAction,
                PagesController.ControllerName,
                new { route = RouteConstants.Home });
        }
    }

    public async Task<IActionResult> SubmitAnswerAndRedirect(
        Controller controller,
        SubmitAnswerInputViewModel answerViewModel,
        string sectionSlug,
        string questionSlug,
        string? returnTo
    )
    {
        var userId = GetUserIdOrThrowException();
        var establishmentId = GetEstablishmentIdOrThrowException();
        var submissionRoutingData = await _submissionService.GetSubmissionRoutingDataAsync(establishmentId, sectionSlug);

        var section = await ContentfulService.GetSectionBySlugAsync(sectionSlug);
        var question = section.GetQuestionBySlug(questionSlug);

        if (!controller.ModelState.IsValid)
        {
            var viewModel = GenerateViewModel(controller, question, section, sectionSlug, answerViewModel.ChosenAnswer?.Answer.Id, returnTo);
            viewModel.ErrorMessages = controller.ModelState
                .Values
                .SelectMany(value => value.Errors.Select(err => err.ErrorMessage))
                .ToArray();

            return controller.View(QuestionView, viewModel);
        }

        try
        {
            await _submissionService.SubmitAnswerAsync(userId, establishmentId, answerViewModel.ToModel());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while submitting an answer with the following message: {Message} ", e.Message);
            var viewModel = GenerateViewModel(controller, question, section, sectionSlug, questionSlug, null);
            viewModel.ErrorMessages = ["Save failed. Please try again later."];
            return controller.View(QuestionView, viewModel);
        }

        var isChangeAnswersFlow = returnTo == FlowConstants.ChangeAnswersFlow;
        if (!isChangeAnswersFlow)
        {
            return await RouteToNextUnansweredQuestion(controller, sectionSlug);
        }

        if (submissionRoutingData.Submission?.Responses is null)
        {
            return controller.RedirectToCheckAnswers(sectionSlug, isChangeAnswersFlow);
        }

        // Check answered questions
        var nextQuestionSlug = GetNextQuestionSlug(submissionRoutingData.Submission.Responses, questionSlug);
        var nextQuestion = section.Questions.FirstOrDefault(q => q.Slug.Equals(nextQuestionSlug));

        // Check unanswered to see if we really have no more questions
        nextQuestion ??= await _questionService.GetNextUnansweredQuestion(establishmentId, section);

        if (nextQuestion != null)
        {
            return await RouteBySlugAndQuestionAsync(controller, sectionSlug, nextQuestion.Slug, FlowConstants.ChangeAnswersFlow);
        }

        // No next questions so check answers
        return controller.RedirectToCheckAnswers(sectionSlug, isChangeAnswersFlow);
    }

    private async Task<string> BuildErrorMessage()
    {
        var contactLink = await ContentfulService.GetLinkByIdAsync(_contactOptions.LinkId);
        var errorMessage = _errorMessages.ConcurrentUsersOrContentChange;

        if (!string.IsNullOrEmpty(contactLink?.Href))
        {
            errorMessage = errorMessage.Replace("contact us", $"<a href=\"{contactLink.Href}\" target=\"_blank\">contact us</a>");
        }

        return errorMessage;
    }

    private QuestionViewModel GenerateViewModel(
        Controller controller,
        CmsQuestionnaireQuestionDto question,
        CmsQuestionnaireSectionDto? section,
        string? sectionSlug,
        string? latestAnswerContentfulId,
        string? returnTo
    )
    {
        controller.ViewData["Title"] = question.Text;

        if (!string.IsNullOrEmpty(returnTo))
        {
            controller.ViewData["ReturnTo"] = returnTo;
        }

        return new QuestionViewModel()
        {
            Question = question,
            AnswerSysId = latestAnswerContentfulId,
            SectionName = section?.Name,
            SectionSlug = sectionSlug,
            SectionId = section?.Id
        };
    }

    private static string? GetNextQuestionSlug(List<QuestionWithAnswerModel> journey, string currentQuestionSlug)
    {
        var currentIndex = journey.FindIndex(q => q.QuestionSlug == currentQuestionSlug);
        if (currentIndex == -1 || currentIndex + 1 >= journey.Count)
        {
            return null;
        }

        // The next question in the valid journey (even if unanswered)
        return journey[currentIndex + 1].QuestionSlug;
    }
}

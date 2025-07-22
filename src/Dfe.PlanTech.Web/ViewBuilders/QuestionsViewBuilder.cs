using Contentful.Core.Models.Management;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Configurations;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class QuestionsViewBuilder(
    ILogger<QuestionsViewBuilder> logger,
    IOptions<ContactOptionsConfiguration> contactOptions,
    IOptions<ErrorPagesConfiguration> errorPages,
    CurrentUser currentUser,
    ContentfulService contentfulService,
    SubmissionService submissionService
) : BaseViewBuilder(currentUser)
{
    private ILogger<QuestionsViewBuilder> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private ContactOptionsConfiguration _contactOptions = contactOptions?.Value ?? throw new ArgumentNullException(nameof(contactOptions));
    private ErrorPagesConfiguration _errorPages = errorPages?.Value ?? throw new ArgumentNullException(nameof(errorPages));
    private ContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    private SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    private const string GetQuestionBySlugActionName = nameof(QuestionsController.GetQuestionBySlug);
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
                sectionSlug,
                submissionRoutingData.NextQuestion!,
                submissionRoutingData.QuestionnaireSection,
                null,
                returnTo
            );
            return controller.View(QuestionView, nextQuestionViewModel);
        }

        if (submissionRoutingData.Status.Equals(SubmissionStatus.NotStarted))
        {
            return PageRedirecter.RedirectToInterstitialPage(controller, sectionSlug);
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
                sectionSlug,
                question,
                submissionRoutingData.QuestionnaireSection,
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

    public IActionResult RouteBasedOnOrganisationType(Controller controller, CmsPageDto page)
    { }

    private QuestionViewModel GenerateViewModel(
        Controller controller,
        string sectionSlug,
        CmsQuestionnaireQuestionDto question,
        CmsQuestionnaireSectionDto section,
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
            AnswerRef = latestAnswerContentfulId,
            SectionName = section?.Name,
            SectionSlug = sectionSlug,
            SectionId = section?.Sys.Id
        };
    }

    /// <summary>
    /// 
    /// </remarks>
    /// <param name="controller"></param>
    /// <param name="submissionRoutingData"></param>
    /// <param name="sectionSlug"></param>
    /// <param name="questionSlug"></param>
    /// <param name="isChangeAnswersFlow"
    /// <returns></returns>
    private async Task<IActionResult> ProcessOtherStatuses(
        Controller controller,
        SubmissionRoutingDataModel submissionRoutingData,
        string sectionSlug,
        string questionSlug,
        bool isChangeAnswersFlow
    )
    {
        
    }
}

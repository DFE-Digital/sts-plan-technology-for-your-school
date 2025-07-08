using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Dfe.PlanTech.Web.Configurations;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Core.Constants;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
[Route("/")]
public class QuestionsController : BaseController<QuestionsController>
{
    public const string Controller = "Questions";
    public const string GetQuestionBySlugActionName = nameof(GetQuestionBySlug);

    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IGetLatestResponsesQuery _getResponseQuery;
    private readonly IGetEntityFromContentfulQuery _getEntityFromContentfulQuery;
    private readonly IGetNavigationQuery _getNavigationQuery;
    private readonly IUser _user;
    private readonly ErrorMessagesConfiguration _errorMessages;
    private readonly ContactOptionsConfiguration _contactOptions;

    public QuestionsController(ILogger<QuestionsController> logger,
                               IGetSectionQuery getSectionQuery,
                               IGetLatestResponsesQuery getResponseQuery,
                               IGetEntityFromContentfulQuery getEntityByIdQuery,
                               IGetNavigationQuery getNavigationQuery,
                               IUser user,
                               IOptions<ErrorMessagesConfiguration> errorMessageOptions,
                               IOptions<ContactOptionsConfiguration> contactOptions) : base(logger)
    {
        _getResponseQuery = getResponseQuery;
        _getSectionQuery = getSectionQuery;
        _getEntityFromContentfulQuery = getEntityByIdQuery;
        _getNavigationQuery = getNavigationQuery;
        _user = user;
        _errorMessages = errorMessageOptions.Value;
        _contactOptions = contactOptions.Value;
    }

    [HttpGet("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> GetQuestionBySlug(string sectionSlug,
                                                    string questionSlug,
                                                    string? returnTo,
                                                    [FromServices] IGetQuestionBySlugRouter router,
                                                    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(questionSlug))
            throw new ArgumentNullException(nameof(questionSlug));

        // Optionally store the returnTo value in TempData or pass it along if router needs it
        TempData["ReturnTo"] = returnTo;

        return await router.ValidateRoute(sectionSlug, questionSlug, this, cancellationToken);
    }

    [LogInvalidModelState]
    [HttpGet("question/preview/{questionId}")]
    public async Task<IActionResult> GetQuestionPreviewById(string questionId,
                                                            [FromServices] ContentfulOptionsConfiguration contentfulOptions,
                                                            CancellationToken cancellationToken = default)
    {
        if (!contentfulOptions.UsePreviewApi)
            return new RedirectResult(UrlConstants.SelfAssessmentPage);

        var question = await _getEntityFromContentfulQuery.GetEntityById<Question>(questionId, cancellationToken) ??
                       throw new ContentfulDataUnavailableException($"Could not find question with Id {questionId}");

        var viewModel = GenerateViewModel(null, question, null, null);
        return RenderView(viewModel);
    }


    [LogInvalidModelState]
    [HttpGet("{sectionSlug}/next-question")]
    public async Task<IActionResult> GetNextUnansweredQuestion(string sectionSlug,
                                                                [FromServices] IGetNextUnansweredQuestionQuery getQuestionQuery,
                                                                [FromServices] IDeleteCurrentSubmissionCommand deleteCurrentSubmissionCommand,
                                                                CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        var section = await GetSectionBySlug(sectionSlug, cancellationToken);

        var establishmentId = await _user.GetEstablishmentId();

        try
        {
            var nextQuestion = await getQuestionQuery.GetNextUnansweredQuestion(establishmentId, section, cancellationToken);

            if (nextQuestion == null)
                return this.RedirectToCheckAnswers(sectionSlug);

            return RedirectToAction(nameof(GetQuestionBySlug), new { sectionSlug, questionSlug = nextQuestion.Slug });
        }
        catch (DatabaseException)
        {
            // Remove the current invalid submission and redirect to self-assessment page
            await deleteCurrentSubmissionCommand.DeleteCurrentSubmission(section, cancellationToken);

            TempData["SubtopicError"] = await BuildErrorMessage();
            return RedirectToAction(
                PagesController.GetPageByRouteAction,
                PagesController.ControllerName,
                new { route = "self-assessment" });
        }
    }

    private async Task<string> BuildErrorMessage()
    {
        var contactLink = await _getNavigationQuery.GetLinkById(_contactOptions.LinkId);
        var errorMessage = _errorMessages.ConcurrentUsersOrContentChange;

        if (contactLink != null && !string.IsNullOrEmpty(contactLink.Href))
        {
            errorMessage = errorMessage.Replace("contact us", $"<a href=\"{contactLink.Href}\" target=\"_blank\">contact us</a>");
        }

        return errorMessage;
    }

    [HttpPost("{sectionSlug}/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(
        string sectionSlug,
        string questionSlug,
        SubmitAnswerViewModel submitAnswerDto,
        [FromServices] ISubmitAnswerCommand submitAnswerCommand,
        [FromServices] IGetNextUnansweredQuestionQuery getQuestionQuery,
        string? returnTo = "",
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = await GenerateViewModel(sectionSlug, questionSlug, cancellationToken);
            viewModel.ErrorMessages = ModelState.Values.SelectMany(value => value.Errors.Select(err => err.ErrorMessage)).ToArray();
            return RenderView(viewModel);
        }

        try
        {
            await submitAnswerCommand.SubmitAnswer(submitAnswerDto, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error has occurred while submitting an answer with the following message: {Message} ", e.Message);
            var viewModel = await GenerateViewModel(sectionSlug, questionSlug, cancellationToken);
            viewModel.ErrorMessages = new[] { "Save failed. Please try again later." };
            return RenderView(viewModel);
        }

        var isChangeAnswersFlow = returnTo == FlowConstants.ChangeAnswersFlow;

        if (isChangeAnswersFlow)
        {
            var establishmentId = await _user.GetEstablishmentId();

            var section = await GetSectionBySlug(sectionSlug, cancellationToken)
                    ?? throw new InvalidOperationException($"Section not found for slug '{sectionSlug}'");

            var submissionResponsesDto = await _getResponseQuery.GetLatestResponses(
                establishmentId,
                section.Sys.Id,
                false,
            cancellationToken);

            if (submissionResponsesDto?.Responses == null)
            {
                return this.RedirectToCheckAnswers(sectionSlug, isChangeAnswersFlow);
            }

            // Check answered questions
            var nextQuestion = GetNextAnsweredQuestion(section, submissionResponsesDto, questionSlug);

            // Check unanswered to see if we really have no more questions
            nextQuestion ??= await getQuestionQuery.GetNextUnansweredQuestion(establishmentId, section, cancellationToken);

            if (nextQuestion != null)
            {
                return RedirectToAction(nameof(GetQuestionBySlug), new
                {
                    sectionSlug,
                    questionSlug = nextQuestion.Slug,
                    returnTo = FlowConstants.ChangeAnswersFlow
                });
            }

            // No next questions so check answers
            return this.RedirectToCheckAnswers(sectionSlug, isChangeAnswersFlow);
        }

        return RedirectToAction(nameof(GetNextUnansweredQuestion), new { sectionSlug });
    }

    [NonAction]
    public IActionResult RenderView(QuestionViewModel viewModel) => View("Question", viewModel);

    [NonAction]
    private async Task<QuestionViewModel> GenerateViewModel(string sectionSlug, string questionSlug, CancellationToken cancellationToken)
    {
        var section = await GetSectionBySlug(sectionSlug, cancellationToken);
        var question = GetQuestionFromSection(section, questionSlug);
        var establishmentId = await _user.GetEstablishmentId();

        var latestResponseForQuestion = await _getResponseQuery.GetLatestResponseForQuestion(establishmentId,
                                                                                section.Sys.Id,
                                                                                question.Sys.Id,
                                                                                cancellationToken);

        return GenerateViewModel(sectionSlug, question, section, latestResponseForQuestion?.AnswerRef);
    }

    [NonAction]
    public QuestionViewModel GenerateViewModel(string? sectionSlug, Question question, ISectionComponent? section, string? latestAnswerContentfulId)
    {
        ViewData["Title"] = question.Text;

        var returnTo = TempData["ReturnTo"]?.ToString();
        if (!string.IsNullOrEmpty(returnTo))
        {
            ViewData["ReturnTo"] = returnTo;
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

    private static Question? GetNextAnsweredQuestion(Section section, SubmissionResponsesDto answeredQuestions, string currentQuestionSlug)
    {
        // Get the valid journey based on latest answers
        var journey = section.GetOrderedResponsesForJourney(answeredQuestions.Responses).ToList();

        // This is the current position in the journey
        var currentIndex = journey.FindIndex(q => q.QuestionSlug == currentQuestionSlug);

        if (currentIndex == -1 || currentIndex + 1 >= journey.Count)
            return null;

        // The next question in the valid journey (even if unanswered)
        var nextQuestionSlug = journey[currentIndex + 1].QuestionSlug;

        return section.Questions.FirstOrDefault(q => q.Slug == nextQuestionSlug);
    }

    public static IActionResult RedirectToQuestionBySlug(string sectionSlug, string questionSlug, Controller controller)
        => controller.RedirectToAction(GetQuestionBySlugActionName, Controller, new { sectionSlug, questionSlug });

    private async Task<Section> GetSectionBySlug(string sectionSlug, CancellationToken cancellationToken)
        => await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
           throw new ContentfulDataUnavailableException($"Could not find section with slug {sectionSlug}");

    private static Question GetQuestionFromSection(Section section, string questionSlug)
        => section.Questions.Find(question => question.Slug == questionSlug) ??
           throw new ContentfulDataUnavailableException($"Could not find question slug {questionSlug} under section {section.Name}");
}

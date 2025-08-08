using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
[Route("/")]
public class QuestionsController : BaseController<QuestionsController>
{
    public const string Controller = "Questions";
    public const string GetQuestionBySlugActionName = nameof(GetQuestionBySlug);

    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IGetLatestResponsesQuery _getResponseQuery;
    private readonly IGetNextUnansweredQuestionQuery _getNextUnansweredQuestionQuery;
    private readonly IUser _user;
    private readonly ErrorMessages _errorMessages;
    private readonly ContactOptions _contactOptions;

    public QuestionsController(ILogger<QuestionsController> logger,
                               IGetSectionQuery getSectionQuery,
                               IGetLatestResponsesQuery getResponseQuery,
                               IGetNextUnansweredQuestionQuery getNextUnansweredQuestionQuery,
                               IUser user,
                               IOptions<ErrorMessages> errorMessageOptions,
                               IOptions<ContactOptions> contactOptions) : base(logger)
    {
        _getResponseQuery = getResponseQuery;
        _getSectionQuery = getSectionQuery;
        _getNextUnansweredQuestionQuery = getNextUnansweredQuestionQuery;
        _user = user;
        _errorMessages = errorMessageOptions.Value;
        _contactOptions = contactOptions.Value;
    }

    [LogInvalidModelState]
    [HttpGet("{categorySlug}/{sectionSlug}/self-assessment/{questionSlug}")]
    public async Task<IActionResult> GetQuestionBySlug(string categorySlug,
                                                    string sectionSlug,
                                                    string questionSlug,
                                                    string? returnTo,
                                                    [FromServices] IGetQuestionBySlugRouter router,
                                                    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(categorySlug))
            throw new ArgumentNullException(nameof(categorySlug));
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));
        if (string.IsNullOrEmpty(questionSlug))
            throw new ArgumentNullException(nameof(questionSlug));

        // Optionally store the returnTo value in TempData or pass it along if router needs it
        TempData["ReturnTo"] = returnTo;

        return await router.ValidateRoute(categorySlug, sectionSlug, questionSlug, this, cancellationToken);
    }

    [LogInvalidModelState]
    [HttpGet("{categorySlug}/{sectionSlug}/self-assessment", Name = "GetInterstitialPage")]
    public async Task<IActionResult> GetInterstitialPage(string categorySlug, string sectionSlug, [FromServices] IGetPageQuery getPageQuery)
    {
        if (string.IsNullOrEmpty(categorySlug))
            throw new ArgumentNullException(nameof(categorySlug));
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        var interstitialPage = await getPageQuery.GetPageBySlug(sectionSlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find interstitial page for section: {sectionSlug}");

        var viewModel = new PageViewModel(interstitialPage, this, _user, Logger);
        return View("~/Views/Pages/Page.cshtml", viewModel);
    }

    [LogInvalidModelState]
    [HttpGet("question/preview/{questionId}")]
    public async Task<IActionResult> GetQuestionPreviewById(string questionId,
                                                            [FromServices] ContentfulOptions contentfulOptions,
                                                            [FromServices] IGetEntityFromContentfulQuery getEntityFromContentfulQuery,
                                                            CancellationToken cancellationToken = default)
    {
        if (!contentfulOptions.UsePreviewApi)
            return new RedirectResult(UrlConstants.HomePage);

        var question = await getEntityFromContentfulQuery.GetEntityById<Question>(questionId, cancellationToken) ??
                       throw new ContentfulDataUnavailableException($"Could not find question with Id {questionId}");

        var viewModel = GenerateViewModel(null, null, question, null, null);
        return RenderView(viewModel);
    }


    [LogInvalidModelState]
    [HttpGet("{categorySlug}/{sectionSlug}/self-assessment/next-question")]
    public async Task<IActionResult> GetNextUnansweredQuestion(string categorySlug,
                                                                string sectionSlug,
                                                                [FromServices] IDeleteCurrentSubmissionCommand deleteCurrentSubmissionCommand,
                                                                [FromServices] IGetNavigationQuery getNavigationQuery,
                                                                CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(categorySlug))
            throw new ArgumentNullException(nameof(categorySlug));
        if (string.IsNullOrEmpty(sectionSlug))
            throw new ArgumentNullException(nameof(sectionSlug));

        var section = await GetSectionBySlug(sectionSlug, cancellationToken);

        var establishmentId = await _user.GetEstablishmentId();

        try
        {
            var nextQuestion = await _getNextUnansweredQuestionQuery.GetNextUnansweredQuestion(establishmentId, section, cancellationToken);

            if (nextQuestion == null)
                return this.RedirectToCheckAnswers(categorySlug, sectionSlug);

            return RedirectToAction(nameof(GetQuestionBySlug), new { categorySlug, sectionSlug, questionSlug = nextQuestion.Slug });
        }
        catch (DatabaseException)
        {
            // Remove the current invalid submission and redirect to homepage
            await deleteCurrentSubmissionCommand.DeleteCurrentSubmission(section, cancellationToken);

            TempData["SubtopicError"] = await BuildErrorMessage(getNavigationQuery);
            return RedirectToAction(
                PagesController.GetPageByRouteAction,
                PagesController.ControllerName,
                new { route = "home" });
        }
    }

    private async Task<string> BuildErrorMessage([FromServices] IGetNavigationQuery getNavigationQuery)
    {
        var contactLink = await getNavigationQuery.GetLinkById(_contactOptions.LinkId);
        var errorMessage = _errorMessages.ConcurrentUsersOrContentChange;

        if (contactLink != null && !string.IsNullOrEmpty(contactLink.Href))
        {
            errorMessage = errorMessage.Replace("contact us", $"<a href=\"{contactLink.Href}\" target=\"_blank\">contact us</a>");
        }

        return errorMessage;
    }

    [HttpPost("{categorySlug}/{sectionSlug}/self-assessment/{questionSlug}")]
    public async Task<IActionResult> SubmitAnswer(
        string categorySlug,
        string sectionSlug,
        string questionSlug,
        SubmitAnswerDto submitAnswerDto,
        [FromServices] ISubmitAnswerCommand submitAnswerCommand,
        string? returnTo = "",
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = await GenerateViewModel(categorySlug, sectionSlug, questionSlug, cancellationToken);
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
            var viewModel = await GenerateViewModel(categorySlug, sectionSlug, questionSlug, cancellationToken);
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
                return this.RedirectToCheckAnswers(categorySlug, sectionSlug, isChangeAnswersFlow);
            }

            // Check answered questions
            var nextQuestion = GetNextAnsweredQuestion(section, submissionResponsesDto, questionSlug);

            // Check unanswered to see if we really have no more questions
            nextQuestion ??= await _getNextUnansweredQuestionQuery.GetNextUnansweredQuestion(establishmentId, section, cancellationToken);

            if (nextQuestion != null)
            {
                return RedirectToAction(nameof(GetQuestionBySlug), new
                {
                    categorySlug,
                    sectionSlug,
                    questionSlug = nextQuestion.Slug,
                    returnTo = FlowConstants.ChangeAnswersFlow
                });
            }

            // No next questions so check answers
            return this.RedirectToCheckAnswers(categorySlug, sectionSlug, isChangeAnswersFlow);
        }

        return RedirectToAction(nameof(GetNextUnansweredQuestion), new { categorySlug, sectionSlug });
    }

    [NonAction]
    public IActionResult RenderView(QuestionViewModel viewModel) => View("Question", viewModel);

    [NonAction]
    private async Task<QuestionViewModel> GenerateViewModel(string categorySlug, string sectionSlug, string questionSlug, CancellationToken cancellationToken)
    {
        var section = await GetSectionBySlug(sectionSlug, cancellationToken);
        var question = GetQuestionFromSection(section, questionSlug);
        var establishmentId = await _user.GetEstablishmentId();

        var latestResponseForQuestion = await _getResponseQuery.GetLatestResponseForQuestion(establishmentId,
                                                                                section.Sys.Id,
                                                                                question.Sys.Id,
                                                                                cancellationToken);

        return GenerateViewModel(categorySlug, sectionSlug, question, section, latestResponseForQuestion?.AnswerRef);
    }

    [NonAction]
    public QuestionViewModel GenerateViewModel(string? categorySlug, string? sectionSlug, Question question, ISectionComponent? section, string? latestAnswerContentfulId)
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
            SectionId = section?.Sys.Id,
            CategorySlug = categorySlug
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

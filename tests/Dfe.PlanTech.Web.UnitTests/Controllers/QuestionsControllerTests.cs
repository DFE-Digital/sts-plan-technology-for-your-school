using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Configurations;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class QuestionsControllerTests
{
    private readonly ILogger<QuestionsController> _logger;
    private readonly IGetNextUnansweredQuestionQuery _getNextUnansweredQuestionQuery;
    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IGetLatestResponsesQuery _getResponseQuery;
    private readonly IGetEntityFromContentfulQuery _getEntityFromContentfulQuery;
    private readonly IGetNavigationQuery _getNavigationQuery;
    private readonly IDeleteCurrentSubmissionCommand _deleteCurrentSubmissionCommand;
    private readonly IGetQuestionBySlugRouter _getQuestionBySlugRouter;
    private readonly IUser _user;
    private readonly IOptions<ErrorMessagesConfiguration> _errorMessages;
    private readonly IOptions<ContactOptionsConfiguration> _contactOptions;
    private readonly QuestionsController _controller;
    private readonly IConfiguration _configuration;

    private const string QuestionSlug = "question-slug";
    private const string SectionSlug = "section-slug";
    private const int EstablishmentId = 1;
    private const string GetNextUnansweredQuestionActionName = "GetNextUnansweredQuestion";
    private const string GetQuestionBySlugActionName = "GetQuestionBySlug";
    private readonly Question _validQuestion = new Question()
    {
        Slug = QuestionSlug,
        Sys = new SystemDetails()
        {
            Id = "QuestionId"
        }
    };

    private readonly Section _validSection = new Section()
    {
        Name = "Valid Section",
        Sys = new SystemDetails()
        {
            Id = "SectionId"
        },
        InterstitialPage = new Page()
        {
            Slug = SectionSlug,
        },
        Questions = new(1)
    };

    public QuestionsControllerTests()
    {
        _validSection.Questions.Add(_validQuestion);

        _logger = Substitute.For<ILogger<QuestionsController>>();
        _configuration = Substitute.For<IConfiguration>();

        _getSectionQuery = Substitute.For<IGetSectionQuery>();
        _getEntityFromContentfulQuery = Substitute.For<IGetEntityFromContentfulQuery>();
        _getNavigationQuery = Substitute.For<IGetNavigationQuery>();

        _getEntityFromContentfulQuery.GetEntityById<Question>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((callinfo) =>
            {
                var questionId = callinfo.ArgAt<string>(0);
                if (questionId == _validQuestion.Sys.Id)
                {
                    return _validQuestion;
                }

                return null;
            });

        _getSectionQuery.GetSectionBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((callInfo) =>
            {
                var sectionSlug = callInfo.ArgAt<string>(0);

                if (sectionSlug == _validSection.InterstitialPage.Slug)
                {
                    return _validSection;
                }

                return null;
            });

        var message = new ErrorMessagesConfiguration
        {
            ConcurrentUsersOrContentChange = "An error occurred. Please contact us."
        };
        _errorMessages = Options.Create(message);


        var contactUs = new ContactOptionsConfiguration
        {
            LinkId = "LinkId"
        };
        _contactOptions = Options.Create(contactUs);

        _getResponseQuery = Substitute.For<IGetLatestResponsesQuery>();
        _getQuestionBySlugRouter = Substitute.For<IGetQuestionBySlugRouter>();
        _getNextUnansweredQuestionQuery = Substitute.For<IGetNextUnansweredQuestionQuery>();
        _deleteCurrentSubmissionCommand = Substitute.For<IDeleteCurrentSubmissionCommand>();

        _user = Substitute.For<IUser>();
        _user.GetEstablishmentId().Returns(EstablishmentId);

        _controller = new QuestionsController(_logger, _getSectionQuery, _getResponseQuery, _getEntityFromContentfulQuery, _getNavigationQuery, _user, _errorMessages, _contactOptions);
        _controller.TempData = Substitute.For<ITempDataDictionary>();
    }

    [Fact]
    public async Task GetQuestionBySlug_Should_Error_When_Missing_SectionId()
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionBySlug(null!, "question-slug", "testReturn", _getQuestionBySlugRouter));
    }

    [Fact]
    public async Task GetQuestionBySlug_Should_Error_When_Missing_QuestionId()
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionBySlug("section-slug", null!, "testReturn", _getQuestionBySlugRouter));
    }

    [Fact]
    public async Task GetQuestionBySlug_Should_Call_Router_When_Args_Valid()
    {
        var sectionSlug = string.Empty;
        var questionSlug = string.Empty;
        QuestionsController? controller = null;

        _getQuestionBySlugRouter.ValidateRoute(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<QuestionsController>(), Arg.Any<CancellationToken>())
                                .Returns((callinfo) =>
                                {
                                    sectionSlug = callinfo.ArgAt<string>(0);
                                    questionSlug = callinfo.ArgAt<string>(1);
                                    controller = callinfo.ArgAt<QuestionsController>(2);

                                    return new AcceptedResult();
                                });

        string section = "section";
        string question = "question";

        await _controller.GetQuestionBySlug(section, question, "testReturn", _getQuestionBySlugRouter);

        Assert.Equal(section, sectionSlug);
        Assert.Equal(question, questionSlug);
        Assert.Equal(_controller, controller);
    }

    [Fact]
    public async Task GetNextUnansweredQuestion_Should_Error_When_SectionSlug_Null()
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetNextUnansweredQuestion(null!, _getNextUnansweredQuestionQuery, _deleteCurrentSubmissionCommand));
    }

    [Fact]
    public async Task GetNextUnansweredQuestion_Should_Error_When_SectionSlug_NotFound()
    {
        var action = () => _controller.GetNextUnansweredQuestion("Not a real section", _getNextUnansweredQuestionQuery, _deleteCurrentSubmissionCommand);
        await Assert.ThrowsAnyAsync<ContentfulDataUnavailableException>(action);
    }

    [Fact]
    public async Task GetNextUnansweredQuestion_Should_Redirect_To_CheckAnswersPage_When_No_Question_Returned()
    {
        var result = await _controller.GetNextUnansweredQuestion(SectionSlug, _getNextUnansweredQuestionQuery, _deleteCurrentSubmissionCommand);

        var redirectResult = result as RedirectToActionResult;
        Assert.NotNull(redirectResult);
        Assert.Equal(CheckAnswersController.ControllerName, redirectResult.ControllerName);
        Assert.Equal(CheckAnswersController.CheckAnswersAction, redirectResult.ActionName);
    }

    [Fact]
    public async Task GetNextUnansweredQuestion_Should_Redirect_To_GetQuestionBySlug_When_NextQuestion_Exsts()
    {
        _getNextUnansweredQuestionQuery.GetNextUnansweredQuestion(EstablishmentId, _validSection, Arg.Any<CancellationToken>())
                                        .Returns((callinfo) => _validQuestion);

        var result = await _controller.GetNextUnansweredQuestion(SectionSlug, _getNextUnansweredQuestionQuery, _deleteCurrentSubmissionCommand);

        var redirectResult = result as RedirectToActionResult;
        Assert.NotNull(redirectResult);
        Assert.Equal(GetQuestionBySlugActionName, redirectResult.ActionName);

        var routeValues = redirectResult.RouteValues;
        Assert.NotNull(routeValues);

        var sectionSlug = routeValues.FirstOrDefault(routeValue => routeValue.Key == "sectionSlug").Value as string;
        var questionSlug = routeValues.FirstOrDefault(routeValue => routeValue.Key == "questionSlug").Value as string;

        Assert.Equal(SectionSlug, sectionSlug);
        Assert.Equal(_validQuestion.Slug, questionSlug);
    }

    [Fact]
    public async Task GetNextUnansweredQuestion_Should_Redirect_To_SelfAssessmentPage_When_Database_Exception_Raised()
    {
        _getNextUnansweredQuestionQuery
            .When(x => x.GetNextUnansweredQuestion(Arg.Any<int>(), Arg.Any<Section>()))
            .Do(_ => throw new DatabaseException("Database exception thrown by the test"));

        _controller.TempData = Substitute.For<ITempDataDictionary>();

        var result = await _controller.GetNextUnansweredQuestion(SectionSlug, _getNextUnansweredQuestionQuery, _deleteCurrentSubmissionCommand);

        var errorMessage = _controller.TempData["SubtopicError"] as string;
        var redirectResult = result as RedirectToActionResult;
        Assert.NotNull(redirectResult);
        Assert.Equal(PagesController.ControllerName, redirectResult.ControllerName);
        Assert.Equal(PagesController.GetPageByRouteAction, redirectResult.ActionName);
        Assert.Contains("Please contact us.", errorMessage);
    }

    [Fact]
    public async Task SubmitAnswer_Should_Return_To_Question_When_Invalid_ModelState()
    {
        var errorMessages = new[] {
      "QuestionId cannot be null",
      "QuestionText cannot be null"
    };
        var sectionSlug = SectionSlug;
        var questionSlug = QuestionSlug;
        var submitAnswerDto = new SubmitAnswerDto();
        var cancellationToken = CancellationToken.None;

        var submitAnswerCommand = Substitute.For<ISubmitAnswerCommand>();
        var nextUnanswered = Substitute.For<IGetNextUnansweredQuestionQuery>();

        _controller.ModelState.AddModelError("submitAnswerDto.QuestionId", errorMessages[0]);
        _controller.ModelState.AddModelError("submitAnswerDto.QuestionText", errorMessages[1]);

        var result = await _controller.SubmitAnswer(sectionSlug, questionSlug, submitAnswerDto, submitAnswerCommand, nextUnanswered, cancellationToken: cancellationToken);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Question", viewResult.ViewName);

        var viewModel = Assert.IsType<QuestionViewModel>(viewResult.Model);

        Assert.NotNull(viewModel);
        Assert.NotNull(viewModel.ErrorMessages);

        foreach (var errorMessage in errorMessages)
        {
            Assert.Contains(errorMessage, viewModel.ErrorMessages);
        }
    }


    [Fact]
    public async Task SubmitAnswer_Should_Handle_Exception_And_Return_InLine_Error_Message()
    {
        var sectionSlug = SectionSlug;
        var questionSlug = QuestionSlug;
        var submitAnswerDto = new SubmitAnswerDto();
        var cancellationToken = CancellationToken.None;

        var submitAnswerCommand = Substitute.For<ISubmitAnswerCommand>();
        var nextUnanswered = Substitute.For<IGetNextUnansweredQuestionQuery>();
        var expectedErrorMessage = "Save failed. Please try again later.";

        submitAnswerCommand
          .When(x => x.SubmitAnswer(Arg.Any<SubmitAnswerDto>(), Arg.Any<CancellationToken>()))
          .Do(x => throw new Exception("A Dummy exception thrown by the test"));

        var result = await _controller.SubmitAnswer(sectionSlug, questionSlug, submitAnswerDto, submitAnswerCommand, nextUnanswered, cancellationToken: cancellationToken);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Question", viewResult.ViewName);

        var viewModel = Assert.IsType<QuestionViewModel>(viewResult.Model);

        Assert.NotNull(viewModel);
        Assert.NotNull(viewModel.ErrorMessages);
        Assert.Contains(expectedErrorMessage, viewModel.ErrorMessages);
    }

    [Fact]
    public async Task SubmitAnswer_Should_Redirect_To_NextUnanswered_When_Not_ChangeAnswersFlow()
    {
        var dto = new SubmitAnswerDto();
        var submitAnswerCommand = Substitute.For<ISubmitAnswerCommand>();
        var nextUnanswered = Substitute.For<IGetNextUnansweredQuestionQuery>();

        submitAnswerCommand.SubmitAnswer(dto, Arg.Any<CancellationToken>()).Returns(1);

        var result = await _controller.SubmitAnswer(SectionSlug, QuestionSlug, dto, submitAnswerCommand, nextUnanswered);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetNextUnansweredQuestion", redirect.ActionName);
        Assert.Equal(SectionSlug, redirect.RouteValues?["sectionSlug"]);
    }

    [Fact]
    public async Task SubmitAnswer_Should_Redirect_To_Next_Answered_Or_Unanswered_When_ChangeAnswersFlow()
    {
        var dto = new SubmitAnswerDto();
        var submitAnswerCommand = Substitute.For<ISubmitAnswerCommand>();
        var nextUnanswered = Substitute.For<IGetNextUnansweredQuestionQuery>();

        submitAnswerCommand.SubmitAnswer(dto, Arg.Any<CancellationToken>()).Returns(1);

        _user.GetEstablishmentId().Returns(EstablishmentId);
        _getResponseQuery.GetLatestResponses(EstablishmentId, _validSection.Sys.Id, false, Arg.Any<CancellationToken>())
            .Returns(new SubmissionResponsesDto
            {
                Responses = new List<QuestionWithAnswer>
                {
                    new QuestionWithAnswer { QuestionRef = _validQuestion.Sys.Id }
                }
            });

        var nextQuestion = new Question { Slug = "next-question" };
        nextUnanswered.GetNextUnansweredQuestion(EstablishmentId, _validSection, Arg.Any<CancellationToken>())
            .Returns(nextQuestion);

        var result = await _controller.SubmitAnswer(SectionSlug, QuestionSlug, dto, submitAnswerCommand, nextUnanswered, returnTo: FlowConstants.ChangeAnswersFlow);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetQuestionBySlug", redirect.ActionName);
        Assert.Equal(SectionSlug, redirect.RouteValues?["sectionSlug"]);
        Assert.Equal("next-question", redirect.RouteValues?["questionSlug"]);
        Assert.Equal(FlowConstants.ChangeAnswersFlow, redirect.RouteValues?["returnTo"]);
    }

    [Fact]
    public async Task SubmitAnswer_Should_Redirect_To_CheckAnswers_When_No_More_Questions_And_ChangeAnswersFlow()
    {
        var dto = new SubmitAnswerDto();
        var submitAnswerCommand = Substitute.For<ISubmitAnswerCommand>();
        var nextUnanswered = Substitute.For<IGetNextUnansweredQuestionQuery>();

        submitAnswerCommand.SubmitAnswer(dto, Arg.Any<CancellationToken>()).Returns(1);

        _user.GetEstablishmentId().Returns(EstablishmentId);
        _getResponseQuery.GetLatestResponses(EstablishmentId, _validSection.Sys.Id, false, Arg.Any<CancellationToken>())
            .Returns(new SubmissionResponsesDto
            {
                Responses = new List<QuestionWithAnswer>()
            });

        var result = await _controller.SubmitAnswer(SectionSlug, QuestionSlug, dto, submitAnswerCommand, nextUnanswered, returnTo: FlowConstants.ChangeAnswersFlow);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("CheckAnswersPage", redirect.ActionName);
        Assert.Equal("CheckAnswers", redirect.ControllerName);
    }


    [Fact]
    public async Task SubmitAnswer_Should_Redirect_To_NextUnansweredQuestion_When_Success()
    {
        var submitAnswerDto = new SubmitAnswerDto();

        var cancellationToken = CancellationToken.None;

        var submitAnswerCommand = Substitute.For<ISubmitAnswerCommand>();
        var nextUnanswered = Substitute.For<IGetNextUnansweredQuestionQuery>();

        submitAnswerCommand.SubmitAnswer(submitAnswerDto, Arg.Any<CancellationToken>())
                          .Returns(1);

        var result = await _controller.SubmitAnswer(SectionSlug, QuestionSlug, submitAnswerDto, submitAnswerCommand, nextUnanswered, cancellationToken: cancellationToken);

        var redirectResult = result as RedirectToActionResult;
        Assert.NotNull(redirectResult);
        Assert.Equal(GetNextUnansweredQuestionActionName, redirectResult.ActionName);

        var routeValues = redirectResult.RouteValues;
        Assert.NotNull(routeValues);

        var sectionSlug = routeValues.FirstOrDefault(routeValue => routeValue.Key == "sectionSlug").Value as string;

        Assert.Equal(SectionSlug, sectionSlug);
    }

    [Fact]
    public async Task QuestionPreview_Should_Redirect_When_UsePreview_Is_False()
    {
        var result = await _controller.GetQuestionPreviewById(_validQuestion.Sys.Id, new ContentfulOptions(false));

        var redirectResult = result as RedirectResult;
        Assert.NotNull(redirectResult);
        Assert.Equal(UrlConstants.SelfAssessmentPage, redirectResult.Url);
    }

    [Fact]
    public async Task QuestionPreview_Should_Return_Valid_Model_With_Section_Omitted_When_UsePreview_Is_True()
    {
        var result = await _controller.GetQuestionPreviewById(_validQuestion.Sys.Id, new ContentfulOptions(true));

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Question", viewResult.ViewName);

        var viewModel = Assert.IsType<QuestionViewModel>(viewResult.Model);

        Assert.NotNull(viewModel);
        Assert.Equal(QuestionSlug, viewModel.Question.Slug);
        Assert.Null(viewModel.SectionSlug);
        Assert.Null(viewModel.AnswerRef);
    }

    [Fact]
    public async Task SubmitAnswer_Should_Return_View_When_ModelState_IsInvalid()
    {
        _controller.ModelState.AddModelError("QuestionId", "QuestionId is required");

        var dto = new SubmitAnswerDto();
        var submitAnswerCommand = Substitute.For<ISubmitAnswerCommand>();
        var nextUnanswered = Substitute.For<IGetNextUnansweredQuestionQuery>();

        var result = await _controller.SubmitAnswer(SectionSlug, QuestionSlug, dto, submitAnswerCommand, nextUnanswered);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(viewResult.Model);
        Assert.Contains("QuestionId is required", model.ErrorMessages);
    }

    [Fact]
    public async Task SubmitAnswer_Should_Return_View_With_Error_When_Exception_Thrown()
    {
        var dto = new SubmitAnswerDto();
        var submitAnswerCommand = Substitute.For<ISubmitAnswerCommand>();
        var nextUnanswered = Substitute.For<IGetNextUnansweredQuestionQuery>();

        submitAnswerCommand
            .When(cmd => cmd.SubmitAnswer(dto, Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("DB error"));

        var result = await _controller.SubmitAnswer(SectionSlug, QuestionSlug, dto, submitAnswerCommand, nextUnanswered);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(viewResult.Model);
        Assert.Contains("Save failed. Please try again later.", model.ErrorMessages);
    }
}

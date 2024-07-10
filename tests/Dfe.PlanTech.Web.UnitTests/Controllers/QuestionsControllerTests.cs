using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class QuestionsControllerTests
{
    private readonly ILogger<QuestionsController> _logger;
    private readonly IGetNextUnansweredQuestionQuery _getNextUnansweredQuestionQuery;
    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IGetLatestResponsesQuery _getResponseQuery;
    private readonly IDeleteCurrentSubmissionCommand _deleteCurrentSubmissionCommand;
    private readonly IGetQuestionBySlugRouter _getQuestionBySlugRouter;
    private readonly IUser _user;
    private readonly QuestionsController _controller;

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

        _getSectionQuery = Substitute.For<IGetSectionQuery>();
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

        _getResponseQuery = Substitute.For<IGetLatestResponsesQuery>();
        _getQuestionBySlugRouter = Substitute.For<IGetQuestionBySlugRouter>();
        _getNextUnansweredQuestionQuery = Substitute.For<IGetNextUnansweredQuestionQuery>();
        _deleteCurrentSubmissionCommand = Substitute.For<IDeleteCurrentSubmissionCommand>();

        _user = Substitute.For<IUser>();
        _user.GetEstablishmentId().Returns(EstablishmentId);

        _controller = new QuestionsController(_logger, _getSectionQuery, _getResponseQuery, _user);
    }

    [Fact]
    public async Task GetQuestionBySlug_Should_Error_When_Missing_SectionId()
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionBySlug(null!, "question-slug", _getQuestionBySlugRouter));
    }

    [Fact]
    public async Task GetQuestionBySlug_Should_Error_When_Missing_QuestionId()
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionBySlug("section-slug", null!, _getQuestionBySlugRouter));
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

        await _controller.GetQuestionBySlug(section, question, _getQuestionBySlugRouter);

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
        await Assert.ThrowsAnyAsync<ContentfulDataUnavailableException>(() => _controller.GetNextUnansweredQuestion("Not a real section", _getNextUnansweredQuestionQuery, _deleteCurrentSubmissionCommand));
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

        var redirectResult = result as RedirectToActionResult;
        Assert.NotNull(redirectResult);
        Assert.Equal(PagesController.ControllerName, redirectResult.ControllerName);
        Assert.Equal(PagesController.GetPageByRouteAction, redirectResult.ActionName);
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

        _controller.ModelState.AddModelError("submitAnswerDto.QuestionId", errorMessages[0]);
        _controller.ModelState.AddModelError("submitAnswerDto.QuestionText", errorMessages[1]);

        var result = await _controller.SubmitAnswer(sectionSlug, questionSlug, submitAnswerDto, submitAnswerCommand, cancellationToken);

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
        var expectedErrorMessage = "Save failed. Please try again later.";

        submitAnswerCommand
          .When(x => x.SubmitAnswer(Arg.Any<SubmitAnswerDto>(), Arg.Any<CancellationToken>()))
          .Do(x => throw new Exception("A Dummy exception thrown by the test"));

        var result = await _controller.SubmitAnswer(sectionSlug, questionSlug, submitAnswerDto, submitAnswerCommand, cancellationToken);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Question", viewResult.ViewName);

        var viewModel = Assert.IsType<QuestionViewModel>(viewResult.Model);

        Assert.NotNull(viewModel);
        Assert.NotNull(viewModel.ErrorMessages);
        Assert.Contains(expectedErrorMessage, viewModel.ErrorMessages);
    }

    [Fact]
    public async Task SubmitAnswer_Should_Redirect_To_NextUnansweredQuestion_When_Success()
    {
        var submitAnswerDto = new SubmitAnswerDto();

        var cancellationToken = CancellationToken.None;

        var submitAnswerCommand = Substitute.For<ISubmitAnswerCommand>();

        submitAnswerCommand.SubmitAnswer(submitAnswerDto, Arg.Any<CancellationToken>())
                          .Returns(1);

        var result = await _controller.SubmitAnswer(SectionSlug, QuestionSlug, submitAnswerDto, submitAnswerCommand, cancellationToken);

        var redirectResult = result as RedirectToActionResult;
        Assert.NotNull(redirectResult);
        Assert.Equal(GetNextUnansweredQuestionActionName, redirectResult.ActionName);

        var routeValues = redirectResult.RouteValues;
        Assert.NotNull(routeValues);

        var sectionSlug = routeValues.FirstOrDefault(routeValue => routeValue.Key == "sectionSlug").Value as string;

        Assert.Equal(SectionSlug, sectionSlug);
    }
}
using Dfe.PlanTech.Application.Questionnaire.Interfaces;
using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Application.Submissions.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
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
  private readonly IUser _user;
  private readonly QuestionsController _controller;

  private const string QUESTION_SLUG = "question-slug";
  private const string SECTION_SLUG = "section-slug";
  private const int ESTABLISHMENT_ID = 1;
    private const string GET_NEXT_UNANSWERED_QUESTION_ACTION_NAME = "GetNextUnansweredQuestion";
    private const string GET_QUESTION_BY_SLUG_ACTION_NAME = "GetQuestionBySlug";
    private readonly Question _validQuestion = new Question()
  {
    Slug = QUESTION_SLUG,
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
      Slug = SECTION_SLUG,
    },
    Questions = new Question[1],
  };

  public QuestionsControllerTests()
  {
    _validSection.Questions[0] = _validQuestion;

    _logger = Substitute.For<ILogger<QuestionsController>>();

    _getSectionQuery = Substitute.For<IGetSectionQuery>();
    _getSectionQuery.GetSectionBySlug(SECTION_SLUG, Arg.Any<CancellationToken>())
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

    _getNextUnansweredQuestionQuery = Substitute.For<IGetNextUnansweredQuestionQuery>();

    _user = Substitute.For<IUser>();
    _user.GetEstablishmentId().Returns(ESTABLISHMENT_ID);

    _controller = new QuestionsController(_logger, _getSectionQuery, _getResponseQuery, _user);
  }

  [Fact]
  public async Task GetQuestionBySlug_Should_Load_QuestionBySlug_When_Args_Valid()
  {
    _getResponseQuery.GetLatestResponseForQuestion(Arg.Any<int>(), _validSection.Sys.Id, _validQuestion.Sys.Id, Arg.Any<CancellationToken>())
            .Returns((callinfo) =>
            {
              QuestionWithAnswer? result = null;

              return result;
            });

    var result = await _controller.GetQuestionBySlug(SECTION_SLUG, QUESTION_SLUG);
    Assert.IsType<ViewResult>(result);

    var viewResult = result as ViewResult;

    var model = viewResult!.Model;

    Assert.IsType<QuestionViewModel>(model);

    var question = model as QuestionViewModel;

    Assert.NotNull(question);
    Assert.Equal(_validQuestion, question.Question);
  }

  [Fact]
  public async Task GetQuestionBySlug_Should_Error_When_Missing_SectionId()
  {
    await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionBySlug(null!, "question-slug"));
  }

  [Fact]
  public async Task GetQuestionBySlug_Should_Error_When_Missing_QuestionId()
  {
    await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionBySlug("section-slug", null!));
  }

  [Fact]
  public async Task GetQuestionBySlug_Should_Error_When_Section_Not_Found()
  {
    await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _controller.GetQuestionBySlug("section", "question"));
  }

  [Fact]
  public async Task GetQuestionBySlug_Should_Error_When_Question_Not_Found()
  {
    await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _controller.GetQuestionBySlug("section-slug", "question"));
  }

  [Fact]
  public async Task GetQuestionBySlug_Should_Display_Page()
  {
    var result = await _controller.GetQuestionBySlug(SECTION_SLUG, QUESTION_SLUG);

    Assert.NotNull(result);

    if (result is not ViewResult view)
    {
      Assert.Fail($"Result is {result.GetType()} but expected {nameof(ViewResult)}");
    }

    var viewResult = result as ViewResult;
    Assert.NotNull(viewResult);
    var model = viewResult.Model as QuestionViewModel;
    Assert.NotNull(model);

    Assert.Equal(_validQuestion, model.Question);
    Assert.Equal(_validSection.Name, model.SectionName);
    Assert.Equal(SECTION_SLUG, model.SectionSlug);
    Assert.Null(model.ErrorMessages);
    Assert.Null(model.AnswerRef);
  }

  [Fact]
  public async Task GetQuestionBySlug_Should_Retrieve_Existing_Answer_And_Display_Page()
  {
    var answerRef = "chosen-answer-ref";
    _getResponseQuery.GetLatestResponseForQuestion(ESTABLISHMENT_ID, _validSection.Sys.Id, _validQuestion.Sys.Id, Arg.Any<CancellationToken>())
                    .Returns((callinfo) => new QuestionWithAnswer()
                    {
                      AnswerRef = answerRef,
                      QuestionRef = _validQuestion.Sys.Id,
                      AnswerText = "answer text",
                      QuestionText = "question text"
                    });

    var result = await _controller.GetQuestionBySlug(SECTION_SLUG, QUESTION_SLUG);

    Assert.NotNull(result);

    if (result is not ViewResult view)
    {
      Assert.Fail($"Result is {result.GetType()} but expected {nameof(ViewResult)}");
    }

    var viewResult = result as ViewResult;
    Assert.NotNull(viewResult);
    var model = viewResult.Model as QuestionViewModel;
    Assert.NotNull(model);

    Assert.Equal(model.Question, _validSection.Questions.First());
    Assert.Equal(model.AnswerRef, answerRef);
  }

  [Fact]
  public async Task GetNextUnansweredQuestion_Should_Error_When_SectionSlug_Null()
  {
    await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetNextUnansweredQuestion(null!, _getNextUnansweredQuestionQuery));
  }

  [Fact]
  public async Task GetNextUnansweredQuestion_Should_Error_When_SectionSlug_NotFound()
  {
    await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _controller.GetNextUnansweredQuestion("Not a real section", _getNextUnansweredQuestionQuery));
  }

  [Fact]
  public async Task GetNextUnansweredQuestion_Should_Redirect_To_CheckAnswersPage_When_No_Question_Returned()
  {
    var result = await _controller.GetNextUnansweredQuestion(SECTION_SLUG, _getNextUnansweredQuestionQuery);

    var redirectResult = result as RedirectToActionResult;
    Assert.NotNull(redirectResult);
    Assert.Equal(PageRedirecter.CHECK_ANSWERS_CONTROLLER, redirectResult.ControllerName);
    Assert.Equal(PageRedirecter.CHECK_ANSWERS_ACTION, redirectResult.ActionName);
  }

  [Fact]
  public async Task GetNextUnansweredQuestion_Should_Redirect_To_GetQuestionBySlug_When_NextQuestion_Exsts()
  {
    _getNextUnansweredQuestionQuery.GetNextUnansweredQuestion(ESTABLISHMENT_ID, _validSection, Arg.Any<CancellationToken>())
                                    .Returns((callinfo) => _validQuestion);

    var result = await _controller.GetNextUnansweredQuestion(SECTION_SLUG, _getNextUnansweredQuestionQuery);

    var redirectResult = result as RedirectToActionResult;
    Assert.NotNull(redirectResult);
    Assert.Equal(GET_QUESTION_BY_SLUG_ACTION_NAME, redirectResult.ActionName);

    var routeValues = redirectResult.RouteValues;
    Assert.NotNull(routeValues);

    var sectionSlug = routeValues.FirstOrDefault(routeValue => routeValue.Key == "sectionSlug").Value as string;
    var questionSlug = routeValues.FirstOrDefault(routeValue => routeValue.Key == "questionSlug").Value as string;

    Assert.Equal(SECTION_SLUG, sectionSlug);
    Assert.Equal(_validQuestion.Slug, questionSlug);
  }

  [Fact]
  public async Task SubmitAnswer_Should_Return_To_Question_When_Invalid_ModelState()
  {
    var errorMessages = new[] {
      "QuestionId cannot be null",
      "QuestionText cannot be null"
    };
    var sectionSlug = SECTION_SLUG;
    var questionSlug = QUESTION_SLUG;
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
    var sectionSlug = SECTION_SLUG;
    var questionSlug = QUESTION_SLUG;
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

    var result = await _controller.SubmitAnswer(SECTION_SLUG, QUESTION_SLUG, submitAnswerDto, submitAnswerCommand, cancellationToken);

    var redirectResult = result as RedirectToActionResult;
    Assert.NotNull(redirectResult);
    Assert.Equal(GET_NEXT_UNANSWERED_QUESTION_ACTION_NAME, redirectResult.ActionName);

    var routeValues = redirectResult.RouteValues;
    Assert.NotNull(routeValues);

    var sectionSlug = routeValues.FirstOrDefault(routeValue => routeValue.Key == "sectionSlug").Value as string;

    Assert.Equal(SECTION_SLUG, sectionSlug);
  }
}
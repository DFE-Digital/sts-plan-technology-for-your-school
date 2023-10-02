using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests;

public class GetQuestionBySlugRouterTests
{
  private readonly IGetLatestResponsesQuery _getResponseQuery = Substitute.For<IGetLatestResponsesQuery>();
  private readonly ISubmissionStatusProcessor _submissionStatusProcessor = Substitute.For<ISubmissionStatusProcessor>();
  private readonly IUser _user = Substitute.For<IUser>();

  private readonly IGetSectionQuery _getSectionQuery = Substitute.For<IGetSectionQuery>();
  private readonly QuestionsController _controller;

  private readonly GetQuestionBySlugRouter _router;

  private readonly ISection _section;

  public GetQuestionBySlugRouterTests()
  {
    _controller = new QuestionsController(new NullLogger<QuestionsController>(), _getSectionQuery, _getResponseQuery, _user);
    _user.GetEstablishmentId().Returns(1);

    var secondQuestion = new Question()
    {
      Slug = "second-question",
      Sys = new SystemDetails()
      {
        Id = "q2"
      },
      Answers = new Answer[]{
        new(){
          Sys = new SystemDetails(){
            Id = "q2-a1"
          }
        }
      }
    };

    var firstQuestion = new Question()
    {
      Slug = "next-question",
      Sys = new SystemDetails()
      {
        Id = "q1"
      },
      Answers = new Answer[]{
        new(){
          Sys = new SystemDetails(){
            Id = "q1-a1"
          },
          NextQuestion = secondQuestion
        },
      }
    };

    var thirdQuestion = new Question()
    {
      Slug = "unattached-question",
      Sys = new SystemDetails()
      {
        Id = "q3"
      },
      Answers = new Answer[]{
        new(){
          Sys = new SystemDetails(){
            Id = "q3-a1"
          }
        }
      }
    };

    _section = new Section()
    {
      Questions = new[] { firstQuestion, secondQuestion, thirdQuestion },
      Name = "section name",
      Sys = new SystemDetails()
      {
        Id = "section-id"
      },
      InterstitialPage = new Page()
      {
        Slug = "section-slug"
      }
    };

    _router = new GetQuestionBySlugRouter(_getResponseQuery, _user, _submissionStatusProcessor);
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  public async Task Should_Throw_Exception_When_SectionSlug_NullOrEmpty(string? sectionSlug)
  {
    await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _router.ValidateRoute(sectionSlug!, "question slug", _controller, default));
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  public async Task Should_Throw_Exception_When_QuestionSlug_NullOrEmpty(string? questionSlug)
  {
    await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _router.ValidateRoute("section soug", questionSlug!, _controller, default));
  }

  [Theory]
  [InlineData(SubmissionStatus.Completed)]
  [InlineData(SubmissionStatus.NextQuestion)]
  [InlineData(SubmissionStatus.NotStarted)]
  public async Task Should_Return_QuestionPage_If_NextQuestion_Matches_Slug(SubmissionStatus submissionStatus)
  {
    var nextQuestion = new Question()
    {
      Slug = "next-question"
    };

    var section = new Section()
    {
      Questions = new[]{
        nextQuestion,
        new Question(){
          Slug = "second-question"
        }
      },
      Name = "section name",
      Sys = new SystemDetails()
      {
        Id = "section-id"
      }
    };

    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                              .Do(callinfo =>
                              {
                                _submissionStatusProcessor.NextQuestion = nextQuestion;
                                _submissionStatusProcessor.Status = submissionStatus;
                                _submissionStatusProcessor.Section.Returns(section);
                              });

    var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, nextQuestion.Slug, _controller, default);

    var pageResult = result as ViewResult;

    Assert.NotNull(pageResult);

    var model = pageResult.Model as QuestionViewModel;

    Assert.NotNull(model);

    Assert.Equal(section.Name, model.SectionName);
    Assert.Equal(_section.InterstitialPage.Slug, model.SectionSlug);
    Assert.Equal(section.Sys.Id, model.SectionId);
    Assert.Null(model.AnswerRef);
    Assert.Equal(nextQuestion, model.Question);
  }

  [Fact]
  public async Task Should_Redirect_To_Correct_QuestionSlug_If_NextQuestion_DoesntMatch_Slug()
  {
    var secondQuestion = _section.Questions[1];
    var nextQuestion = _section.Questions[0];

    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                              .Do(callinfo =>
                              {
                                _submissionStatusProcessor.NextQuestion = secondQuestion;
                                _submissionStatusProcessor.Status = SubmissionStatus.NextQuestion;
                                _submissionStatusProcessor.Section.Returns(_section);
                              });

    var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, nextQuestion.Slug, _controller, default);

    var redirectResult = result as RedirectToActionResult;

    Assert.NotNull(redirectResult);

    Assert.Equal(QuestionsController.Controller, redirectResult.ControllerName);
    Assert.Equal("GetQuestionBySlug", redirectResult.ActionName);

    var sectionSlug = redirectResult.RouteValues?["sectionSlug"];
    var questionSlug = redirectResult.RouteValues?["questionSlug"];

    Assert.NotNull(sectionSlug);
    Assert.NotNull(questionSlug);
    Assert.Equal(_section.InterstitialPage.Slug, sectionSlug);
    Assert.Equal(secondQuestion.Slug, questionSlug);
  }

  [Fact]
  public async Task Should_Redirect_To_CheckAnswers_When_Status_Is_CheckAnswers_And_Question_Is_Unattached()
  {
    var firstQuestion = _section.Questions[0];
    var secondQuestion = _section.Questions[1];
    var thirdQuestion = _section.Questions[2];

    var responses = new CheckAnswerDto()
    {
      Responses = new List<QuestionWithAnswer>(){
        new(){
          QuestionRef = firstQuestion.Sys.Id,
          AnswerRef = firstQuestion.Answers[0].Sys.Id
        },
        new(){
          QuestionRef = secondQuestion.Sys.Id,
          AnswerRef = secondQuestion.Answers[0].Sys.Id
        }
      }
    };

    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                              .Do(callinfo =>
                              {
                                _submissionStatusProcessor.Status = SubmissionStatus.CheckAnswers;
                                _submissionStatusProcessor.Section.Returns(_section);
                              });

    _getResponseQuery.GetLatestResponses(Arg.Any<int>(), _section.Sys.Id, Arg.Any<CancellationToken>())
                     .Returns(responses);

    var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, thirdQuestion.Slug, _controller, default);

    var redirectResult = result as RedirectToActionResult;

    Assert.NotNull(redirectResult);

    Assert.Equal(CheckAnswersController.ControllerName, redirectResult.ControllerName);
    Assert.Equal(CheckAnswersController.CheckAnswersAction, redirectResult.ActionName);

    var sectionSlug = redirectResult.RouteValues?["sectionSlug"];

    Assert.NotNull(sectionSlug);
    Assert.Equal(_section.InterstitialPage.Slug, sectionSlug);
  }
}
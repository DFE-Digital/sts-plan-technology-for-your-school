using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Routing;

public class GetRecommendationRouterTests
{
  private readonly IGetPageQuery _getPageQuery;
  private readonly ISubmissionStatusProcessor _submissionStatusProcessor;
  private readonly IUser _user;

  private readonly RecommendationsController _controller;
  private readonly IGetRecommendationRouter _router;

  private readonly ISection _section = new Section()
  {
    InterstitialPage = new Page()
    {
      Slug = "section-slug"
    },
    Sys = new SystemDetails()
    {
      Id = "section-id"
    },
    Recommendations = new RecommendationPage[]{
      new(){
        Page = new Page(){
          Slug = "low-recommendation-slug"
        },
        Maturity = Maturity.Low,
        Sys = new SystemDetails(){
          Id = "low-id"
        }
      },
      new(){
        Page = new Page(){
          Slug = "high-recommendation-slug"
        },
        Maturity = Maturity.High,
        Sys = new SystemDetails(){
          Id = "high-id"
        }
      }

    }
  };

  public GetRecommendationRouterTests()
  {
    _getPageQuery = Substitute.For<IGetPageQuery>();
    _submissionStatusProcessor = Substitute.For<ISubmissionStatusProcessor>();
    _user = Substitute.For<IUser>();

    _controller = new RecommendationsController(new NullLogger<RecommendationsController>());

    _getPageQuery.GetPageBySlug(_section.Recommendations[0].Page.Slug, Arg.Any<CancellationToken>())
                 .Returns(_section.Recommendations[0].Page);

    _router = new GetRecommendationRouter(_getPageQuery, new NullLogger<GetRecommendationRouter>(), _user, _submissionStatusProcessor);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  public async Task Should_ThrowException_When_SectionSlug_NullOrEmpty(string? sectionSlug)
  {
    await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _router.ValidateRoute(sectionSlug!, "recommendation-slug", _controller, default));
  }


  [Theory]
  [InlineData(null)]
  [InlineData("")]
  public async Task Should_ThrowException_When_RecommendationSlug_NullOrEmpty(string? recommendationSlug)
  {
    await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _router.ValidateRoute("section-slug", recommendationSlug!, _controller, default));
  }

  [Fact]
  public async Task Should_Redirect_To_CheckAnswersPage_When_Status_CheckAnswers()
  {
    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                              .Do((callinfo) =>
                              {
                                _submissionStatusProcessor.Status = SubmissionStatus.CheckAnswers;
                              });

    var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "recommendation-slug", _controller, default);

    var redirectResult = result as RedirectToActionResult;

    Assert.NotNull(redirectResult);

    Assert.Equal(CheckAnswersController.ControllerName, redirectResult.ControllerName);
    Assert.Equal(CheckAnswersController.CheckAnswersAction, redirectResult.ActionName);

    var section = redirectResult.RouteValues?["sectionSlug"];

    Assert.NotNull(section);
    Assert.Equal(_section.InterstitialPage.Slug, section);
  }

  [Theory]
  [InlineData(SubmissionStatus.NotStarted)]
  [InlineData(SubmissionStatus.NextQuestion)]
  public async Task Should_Redirect_To_QuestionBySlug_When_Status_NotStarted_Or_NextQuestion(SubmissionStatus submissionStatus)
  {
    var nextQuestion = new Question()
    {
      Slug = "next-question"
    };

    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                              .Do((callinfo) =>
                              {
                                _submissionStatusProcessor.Status = submissionStatus;
                                _submissionStatusProcessor.NextQuestion = nextQuestion;
                              });

    var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "recommendation-slug", _controller, default);

    var redirectResult = result as RedirectToActionResult;

    Assert.NotNull(redirectResult);

    Assert.Equal(QuestionsController.Controller, redirectResult.ControllerName);
    Assert.Equal(QuestionsController.GetQuestionBySlugActionName, redirectResult.ActionName);

    var section = redirectResult.RouteValues?["sectionSlug"];

    Assert.NotNull(section);
    Assert.Equal(_section.InterstitialPage.Slug, section);

    var questionSlug = redirectResult.RouteValues?["questionSlug"];

    Assert.NotNull(questionSlug);
    Assert.Equal(nextQuestion.Slug, questionSlug);
  }

  [Fact]
  public async Task Should_Throw_Exception_When_Maturity_Null()
  {
    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                              .Do((callinfo) =>
                              {
                                _submissionStatusProcessor.Status = SubmissionStatus.Completed;
                                _submissionStatusProcessor.SectionStatus.Returns(new SectionStatusNew()
                                {
                                  Maturity = null
                                });
                              });

    await Assert.ThrowsAnyAsync<DatabaseException>(() => _router.ValidateRoute(_section.InterstitialPage.Slug, "recommendation-slug", _controller, default));
  }

  [Fact]
  public async Task Should_Throw_Exception_When_Recommendation_Not_In_Section()
  {
    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                              .Do((callinfo) =>
                              {
                                _submissionStatusProcessor.Status = SubmissionStatus.Completed;
                                _submissionStatusProcessor.Section.Returns(_section);
                                _submissionStatusProcessor.SectionStatus.Returns(new SectionStatusNew()
                                {
                                  Maturity = "High"
                                });
                              });

    await Assert.ThrowsAnyAsync<ContentfulDataUnavailableException>(() => _router.ValidateRoute(_section.InterstitialPage.Slug, "other-recommendation-slug", _controller, default));
  }

  [Fact]
  public async Task Should_Throw_Exception_When_NotFind_Recommendation_For_Maturity()
  {
    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                              .Do((callinfo) =>
                              {
                                _submissionStatusProcessor.Status = SubmissionStatus.Completed;
                                _submissionStatusProcessor.Section.Returns(_section);
                                _submissionStatusProcessor.SectionStatus.Returns(new SectionStatusNew()
                                {
                                  Maturity = "not a real maturity"
                                });
                              });

    await Assert.ThrowsAnyAsync<ContentfulDataUnavailableException>(() => _router.ValidateRoute(_section.InterstitialPage.Slug, _section.Recommendations[0].Page.Slug, _controller, default));
  }

  [Fact]
  public async Task Should_Redirect_To_Correct_Recommendation_Page_When_Recommendation_Not_Match()
  {
    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                              .Do((callinfo) =>
                              {
                                _submissionStatusProcessor.Status = SubmissionStatus.Completed;
                                _submissionStatusProcessor.Section.Returns(_section);
                                _submissionStatusProcessor.SectionStatus.Returns(new SectionStatusNew()
                                {
                                  Maturity = "High"
                                });
                              });

    var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, _section.Recommendations[0].Page.Slug, _controller, default);

    var redirectResult = result as RedirectToActionResult;

    Assert.NotNull(redirectResult);

    Assert.Equal(RecommendationsController.GetRecommendationAction, redirectResult.ActionName);

    var section = redirectResult.RouteValues?["sectionSlug"];

    Assert.NotNull(section);
    Assert.Equal(_section.InterstitialPage.Slug, section);

    var recommendationSlug = redirectResult.RouteValues?["recommendationSlug"];

    Assert.NotNull(recommendationSlug);
    Assert.Equal(_section.Recommendations[1].Page.Slug, recommendationSlug);
  }

  [Fact]
  public async Task Should_Show_RecommendationPage_When_Status_Is_Recommendation_And_All_Valid()
  {
    var recommendation = _section.Recommendations[0];

    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                              .Do((callinfo) =>
                              {
                                _submissionStatusProcessor.Status = SubmissionStatus.Completed;
                                _submissionStatusProcessor.Section.Returns(_section);
                                _submissionStatusProcessor.SectionStatus.Returns(new SectionStatusNew()
                                {
                                  Maturity = "Low"
                                });
                              });

    var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, recommendation.Page.Slug, _controller, default);

    var viewResult = result as ViewResult;

    Assert.NotNull(viewResult);

    var model = viewResult.Model as PageViewModel;

    Assert.NotNull(model);
    Assert.Equal(model.Page, recommendation.Page);
  }

}
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

  private readonly string _sectionSlug = "section-slug";

  public GetQuestionBySlugRouterTests()
  {
    _controller = new QuestionsController(new NullLogger<QuestionsController>(), _getSectionQuery, _getResponseQuery, _user);

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

    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_sectionSlug, Arg.Any<CancellationToken>()))
                              .Do(callinfo =>
                              {
                                _submissionStatusProcessor.NextQuestion = nextQuestion;
                                _submissionStatusProcessor.Status = submissionStatus;
                                _submissionStatusProcessor.Section.Returns(section);
                              });

    var result = await _router.ValidateRoute(_sectionSlug, nextQuestion.Slug, _controller, default);

    var pageResult = result as ViewResult;

    Assert.NotNull(pageResult);

    var model = pageResult.Model as QuestionViewModel;

    Assert.NotNull(model);

    Assert.Equal(section.Name, model.SectionName);
    Assert.Equal(_sectionSlug, model.SectionSlug);
    Assert.Equal(section.Sys.Id, model.SectionId);
    Assert.Null(model.AnswerRef);
    Assert.Equal(nextQuestion, model.Question);
  }

  [Fact]
  public async Task Should_Redirect_To_Correct_QuestionSlug_If_NextQuestion_DoesntMatch_Slug()
  {
    var nextQuestion = new Question()
    {
      Slug = "next-question"
    };

    var secondQuestion = new Question()
    {
      Slug = "second-question"
    };

    var section = new Section()
    {
      Questions = new[] { nextQuestion, secondQuestion },
      Name = "section name",
      Sys = new SystemDetails()
      {
        Id = "section-id"
      }
    };

    _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_sectionSlug, Arg.Any<CancellationToken>()))
                              .Do(callinfo =>
                              {
                                _submissionStatusProcessor.NextQuestion = secondQuestion;
                                _submissionStatusProcessor.Status = SubmissionStatus.NextQuestion;
                                _submissionStatusProcessor.Section.Returns(section);
                              });

    var result = await _router.ValidateRoute(_sectionSlug, nextQuestion.Slug, _controller, default);

    var redirectResult = result as RedirectToActionResult;

    Assert.NotNull(redirectResult);

    Assert.Equal(QuestionsController.Controller, redirectResult.ControllerName);
    Assert.Equal("GetQuestionBySlug", redirectResult.ActionName);

    var sectionSlug = redirectResult.RouteValues?["sectionSlug"];
    var questionSlug = redirectResult.RouteValues?["questionSlug"];

    Assert.NotNull(sectionSlug);
    Assert.NotNull(questionSlug);
    Assert.Equal(_sectionSlug, sectionSlug);
    Assert.Equal(secondQuestion.Slug, questionSlug);
  }

}
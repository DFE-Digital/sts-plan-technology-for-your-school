using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
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

namespace Dfe.PlanTech.Web.UnitTests;

public class GetQuestionBySlugRouterTests
{
    private readonly IGetLatestResponsesQuery _getResponseQuery = Substitute.For<IGetLatestResponsesQuery>();
    private readonly ISubmissionStatusProcessor _submissionStatusProcessor = Substitute.For<ISubmissionStatusProcessor>();
    private readonly IUser _user = Substitute.For<IUser>();

    private readonly IGetSectionQuery _getSectionQuery = Substitute.For<IGetSectionQuery>();
    private readonly QuestionsController _controller;

    private readonly GetQuestionBySlugRouter _router;

    private readonly Section _section;

    private readonly SubmissionResponsesDto _responses;

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
            Answers = new(){
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
            Answers = new(){
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
            Answers = new(){
        new(){
          Sys = new SystemDetails(){
            Id = "q3-a1"
          }
        }
      }
        };

        _section = new Section()
        {
            Questions = new() { firstQuestion, secondQuestion, thirdQuestion },
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

        _responses = new()
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
        var nextQuestion = _section.Questions[0];

        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                                  .Do(callinfo =>
                                  {
                                      _submissionStatusProcessor.NextQuestion = nextQuestion;
                                      _submissionStatusProcessor.Status = submissionStatus;
                                      _submissionStatusProcessor.Section.Returns(_section);
                                  });

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, nextQuestion.Slug, _controller, default);

        var viewResult = result as ViewResult;

        Assert.NotNull(viewResult);

        var model = viewResult.Model as QuestionViewModel;

        Assert.NotNull(model);

        Assert.Equal(_section.Name, model.SectionName);
        Assert.Equal(_section.InterstitialPage.Slug, model.SectionSlug);
        Assert.Equal(_section.Sys.Id, model.SectionId);
        Assert.Null(model.AnswerRef);
        Assert.Equal(nextQuestion, model.Question);
    }

    [Fact]
    public async Task Should_Redirect_To_Correct_QuestionSlug_If_NextQuestion_DoesntMatch_Slug_And_Question_Is_Not_Attached()
    {
        var thirdQuestion = _section.Questions[2];
        var secondQuestion = _section.Questions[1];
        var firstQuestion = _section.Questions[0];

        _getResponseQuery.GetLatestResponses(Arg.Any<int>(), _section.Sys.Id, false, Arg.Any<CancellationToken>())
                         .Returns(_responses);

        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                                  .Do(callinfo =>
                                  {
                                      _submissionStatusProcessor.NextQuestion = secondQuestion;
                                      _submissionStatusProcessor.Status = SubmissionStatus.NextQuestion;
                                      _submissionStatusProcessor.Section.Returns(_section);
                                  });

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, thirdQuestion.Slug, _controller, default);

        var redirectResult = result as RedirectToActionResult;

        Assert.NotNull(redirectResult);

        Assert.Equal(QuestionsController.Controller, redirectResult.ControllerName);
        Assert.Equal(QuestionsController.GetQuestionBySlugActionName, redirectResult.ActionName);

        var sectionSlug = redirectResult.RouteValues?["sectionSlug"];
        var questionSlug = redirectResult.RouteValues?["questionSlug"];

        Assert.NotNull(sectionSlug);
        Assert.NotNull(questionSlug);
        Assert.Equal(_section.InterstitialPage.Slug, sectionSlug);
        Assert.Equal(secondQuestion.Slug, questionSlug);
    }

    [Fact]
    public async Task Should_Show_Question_Page_If_Status_Is_NextQuestion_And_Is_PreviouslyAnsweredQuestion_In_Journey()
    {
        var thirdQuestion = _section.Questions[2];
        var firstQuestion = _section.Questions[0];

        var responseForQuestion = _responses.Responses.First(resp => resp.QuestionRef == firstQuestion.Sys.Id);

        _getResponseQuery.GetLatestResponses(Arg.Any<int>(), _section.Sys.Id, false, Arg.Any<CancellationToken>())
                         .Returns(_responses);

        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                                  .Do(callinfo =>
                                  {
                                      _submissionStatusProcessor.NextQuestion = thirdQuestion;
                                      _submissionStatusProcessor.Status = SubmissionStatus.NextQuestion;
                                      _submissionStatusProcessor.Section.Returns(_section);
                                  });

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, firstQuestion.Slug, _controller, default);

        var viewResult = result as ViewResult;

        Assert.NotNull(viewResult);

        var model = viewResult.Model as QuestionViewModel;

        Assert.NotNull(model);

        Assert.Equal(_section.Name, model.SectionName);
        Assert.Equal(_section.InterstitialPage.Slug, model.SectionSlug);
        Assert.Equal(_section.Sys.Id, model.SectionId);
        Assert.Equal(responseForQuestion.AnswerRef, model.AnswerRef);
        Assert.Equal(firstQuestion, model.Question);
    }


    [Theory]
    [InlineData(SubmissionStatus.Completed)]
    [InlineData(SubmissionStatus.NotStarted)]
    public async Task Should_Redirect_To_InterstitialPage_When_QuestionSlug_Not_Matching_And_Status_CompeletedOrNotStarted(SubmissionStatus submissionStatus)
    {
        var secondQuestion = _section.Questions[1];
        var nextQuestion = _section.Questions[0];

        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                                  .Do(callinfo =>
                                  {
                                      _submissionStatusProcessor.NextQuestion = secondQuestion;
                                      _submissionStatusProcessor.Status = submissionStatus;
                                      _submissionStatusProcessor.Section.Returns(_section);
                                  });

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, nextQuestion.Slug, _controller, default);

        var redirectResult = result as RedirectToActionResult;

        Assert.NotNull(redirectResult);

        Assert.Equal(PagesController.ControllerName, redirectResult.ControllerName);
        Assert.Equal(PagesController.GetPageByRouteAction, redirectResult.ActionName);

        var route = redirectResult.RouteValues?["route"];
        Assert.NotNull(route);
        Assert.Equal(_section.InterstitialPage.Slug, route);
    }

    [Fact]
    public async Task Should_Redirect_To_CheckAnswers_When_Status_Is_CheckAnswers_And_Question_Is_Unattached()
    {
        var firstQuestion = _section.Questions[0];
        var secondQuestion = _section.Questions[1];
        var thirdQuestion = _section.Questions[2];

        _getResponseQuery.GetLatestResponses(Arg.Any<int>(), _section.Sys.Id, false, Arg.Any<CancellationToken>())
                         .Returns(_responses);

        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                                  .Do(callinfo =>
                                  {
                                      _submissionStatusProcessor.Status = SubmissionStatus.CheckAnswers;
                                      _submissionStatusProcessor.Section.Returns(_section);
                                  });

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, thirdQuestion.Slug, _controller, default);

        var redirectResult = result as RedirectToActionResult;

        Assert.NotNull(redirectResult);

        Assert.Equal(CheckAnswersController.ControllerName, redirectResult.ControllerName);
        Assert.Equal(CheckAnswersController.CheckAnswersAction, redirectResult.ActionName);

        var sectionSlug = redirectResult.RouteValues?["sectionSlug"];

        Assert.NotNull(sectionSlug);
        Assert.Equal(_section.InterstitialPage.Slug, sectionSlug);
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Question_NoLonger_In_Section()
    {
        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                                    .Do(callinfo =>
                                    {
                                        _submissionStatusProcessor.Status = SubmissionStatus.CheckAnswers;
                                        _submissionStatusProcessor.Section.Returns(_section);
                                    });

        _getResponseQuery.GetLatestResponses(Arg.Any<int>(), _section.Sys.Id, false, Arg.Any<CancellationToken>())
                          .Returns(_responses);


        await Assert.ThrowsAnyAsync<ContentfulDataUnavailableException>(() => _router.ValidateRoute(_section.InterstitialPage.Slug, "fourth-question", _controller, default));
    }

    [Fact]
    public async Task Should_Throw_Exception_When_No_Responses_Returned()
    {
        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                                .Do(callinfo =>
                                {
                                    _submissionStatusProcessor.Status = SubmissionStatus.CheckAnswers;
                                    _submissionStatusProcessor.Section.Returns(_section);
                                });

        await Assert.ThrowsAnyAsync<DatabaseException>(() => _router.ValidateRoute(_section.InterstitialPage.Slug, _section.Questions[0].Slug, _controller, default));
    }

    [Fact]
    public async Task Should_Return_QuestionPage_When_CheckAnswers_And_Attached_Question()
    {
        var firstQuestion = _section.Questions[0];

        var responseForQuestion = _responses.Responses.First(resp => resp.QuestionRef == firstQuestion.Sys.Id);

        _getResponseQuery.GetLatestResponses(Arg.Any<int>(), _section.Sys.Id, false, Arg.Any<CancellationToken>())
                         .Returns(_responses);

        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                                  .Do(callinfo =>
                                  {
                                      _submissionStatusProcessor.Status = SubmissionStatus.CheckAnswers;
                                      _submissionStatusProcessor.Section.Returns(_section);
                                  });

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, firstQuestion.Slug, _controller, default);

        var viewResult = result as ViewResult;

        Assert.NotNull(viewResult);

        var model = viewResult.Model as QuestionViewModel;

        Assert.NotNull(model);

        Assert.Equal(_section.Name, model.SectionName);
        Assert.Equal(_section.InterstitialPage.Slug, model.SectionSlug);
        Assert.Equal(_section.Sys.Id, model.SectionId);
        Assert.Equal(responseForQuestion.AnswerRef, model.AnswerRef);
        Assert.Equal(firstQuestion, model.Question);
    }
}
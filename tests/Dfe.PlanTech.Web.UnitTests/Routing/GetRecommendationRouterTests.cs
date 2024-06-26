using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.Core;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Routing;

public class GetRecommendationRouterTests
{
    private readonly ISubmissionStatusProcessor _submissionStatusProcessor;

    private readonly RecommendationsController _controller;
    private readonly GetRecommendationRouter _router;

    private readonly IGetLatestResponsesQuery _getLatestResponsesQuery;
    private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery;

    private static readonly Answer AnswerOne = new()
    {
        Sys = new()
        {
            Id = "Answer-1"
        },
        Text = "Answer-1-Text"
    };

    private static readonly Answer AnswerTwo = new()
    {
        Sys = new()
        {
            Id = "Answer-2"
        },
        Text = "Answer-2-Text"
    };

    private static readonly Question Question = new()
    {
        Sys = new()
        {
            Id = "Question-1"
        },
        Text = "Question-Text",
        Slug = "Question-1-Slug",
        Answers = [AnswerOne, AnswerTwo]
    };

    private readonly Section _section = new()
    {
        InterstitialPage = new Page()
        {
            Slug = "section-slug"
        },
        Sys = new SystemDetails()
        {
            Id = "section-id"
        },
        Questions = [Question]
    };

    private readonly SubtopicRecommendation? _subtopic = new()
    {
        Intros =
        [
            new()
            {
                Slug = "intro-slug",
                Maturity = "High",
            }
        ],
        Section = new RecommendationSection()
        {
            Chunks =
            [
                new()
                {
                    Header = new()
                    {
                        Text = "test-header"
                    },
                    Answers = [AnswerOne]
                }
            ]
        },
        Subtopic = new Section()
        {
            InterstitialPage = new Page()
            {
                Slug = "subtopic-slug"
            },
            Sys = new SystemDetails()
            {
                Id = "subtopic-id"
            },
        }
    };

    public GetRecommendationRouterTests()
    {
        _submissionStatusProcessor = Substitute.For<ISubmissionStatusProcessor>();
        _getLatestResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
        _getSubTopicRecommendationQuery = Substitute.For<IGetSubTopicRecommendationQuery>();

        _controller = new RecommendationsController(new NullLogger<RecommendationsController>());

        _router = new GetRecommendationRouter(_submissionStatusProcessor, _getLatestResponsesQuery, _getSubTopicRecommendationQuery);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData("", false)]
    [InlineData("", true)]
    public async Task Should_ThrowException_When_SectionSlug_NullOrEmpty(string? sectionSlug, bool checklist)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() =>
            _router.ValidateRoute(sectionSlug!, "recommendation-slug", checklist, _controller, default));
    }


    [Theory]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData("", false)]
    [InlineData("", true)]
    public async Task Should_ThrowException_When_RecommendationSlug_NullOrEmpty(string? recommendationSlug, bool checklist)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() =>
            _router.ValidateRoute("section-slug", recommendationSlug!, checklist, _controller, default));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_Redirect_To_CheckAnswersPage_When_Status_CheckAnswers(bool checklist)
    {
        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSectionRecommendation(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
                                  .Do((callinfo) => { _submissionStatusProcessor.Status = SubmissionStatus.CheckAnswers; });

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "recommendation-slug", checklist, _controller,
            default);

        var redirectResult = result as RedirectToActionResult;

        Assert.NotNull(redirectResult);

        Assert.Equal(CheckAnswersController.ControllerName, redirectResult.ControllerName);
        Assert.Equal(CheckAnswersController.CheckAnswersAction, redirectResult.ActionName);

        var section = redirectResult.RouteValues?["sectionSlug"];

        Assert.NotNull(section);
        Assert.Equal(_section.InterstitialPage.Slug, section);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_Redirect_To_QuestionBySlug_When_Status_NextQuestion(bool checklist)
    {
        var nextQuestion = new Question()
        {
            Slug = "next-question"
        };

        _submissionStatusProcessor.When(processor =>
                processor.GetJourneyStatusForSectionRecommendation(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
            .Do((callinfo) =>
            {
                _submissionStatusProcessor.Status = SubmissionStatus.NextQuestion;
                _submissionStatusProcessor.NextQuestion = nextQuestion;
            });

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "recommendation-slug", checklist, _controller,
            default);

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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_Redirect_To_InterstitialPage_When_NotStarted(bool checklist)
    {
        var nextQuestion = new Question()
        {
            Slug = "next-question"
        };

        _submissionStatusProcessor.When(processor =>
                processor.GetJourneyStatusForSectionRecommendation(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
            .Do((callinfo) =>
            {
                _submissionStatusProcessor.Status = SubmissionStatus.NotStarted;
                _submissionStatusProcessor.NextQuestion = nextQuestion;
            });

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "recommendation-slug", checklist, _controller,
            default);

        var redirectResult = result as RedirectToActionResult;

        Assert.NotNull(redirectResult);

        Assert.Equal(PagesController.ControllerName, redirectResult.ControllerName);
        Assert.Equal(PagesController.GetPageByRouteAction, PagesController.GetPageByRouteAction);

        var route = redirectResult.RouteValues?["route"];
        Assert.NotNull(route);
        Assert.Equal(PageRedirecter.SelfAssessmentRoute, route);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_Throw_Exception_When_Maturity_Null(bool checklist)
    {
        _submissionStatusProcessor.When(processor =>
                processor.GetJourneyStatusForSectionRecommendation(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
            .Do((callinfo) =>
            {
                _submissionStatusProcessor.Status = SubmissionStatus.Completed;
                _submissionStatusProcessor.SectionStatus.Returns(new SectionStatusNew()
                {
                    Maturity = null
                });
            });

        await Assert.ThrowsAnyAsync<DatabaseException>(() =>
            _router.ValidateRoute(_section.InterstitialPage.Slug, "recommendation-slug", checklist, _controller, default));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_Throw_Exception_When_Recommendation_Not_In_Section(bool checklist)
    {
        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSectionRecommendation(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
            .Do((callinfo) =>
            {
                _submissionStatusProcessor.Status = SubmissionStatus.Completed;
                _submissionStatusProcessor.Section.Returns(_section);
                _submissionStatusProcessor.SectionStatus.Returns(new SectionStatusNew()
                {
                    Maturity = "High"
                });
            });

        _getLatestResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                                .Returns(MockValidLatestResponse);

        await Assert.ThrowsAnyAsync<ContentfulDataUnavailableException>(() =>
            _router.ValidateRoute(_section.InterstitialPage.Slug, "other-recommendation-slug", checklist, _controller, default));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_Throw_Exception_When_NotFind_Recommendation_For_Maturity(bool checklist)
    {
        _submissionStatusProcessor.When(processor =>
                processor.GetJourneyStatusForSectionRecommendation(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
            .Do((callinfo) =>
            {
                _submissionStatusProcessor.Status = SubmissionStatus.Completed;
                _submissionStatusProcessor.Section.Returns(_section);
                _submissionStatusProcessor.SectionStatus.Returns(new SectionStatusNew()
                {
                    Maturity = "not a real maturity"
                });
            });

        _getLatestResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                                .Returns(MockValidLatestResponse);

        _getSubTopicRecommendationQuery.GetSubTopicRecommendation(Arg.Any<string>()).Returns(_subtopic);

        await Assert.ThrowsAnyAsync<ContentfulDataUnavailableException>(() =>
            _router.ValidateRoute(_section.InterstitialPage.Slug, "any-recommendation-slug", checklist, _controller,
                default));
    }

    [Fact]
    public async Task Should_Show_RecommendationPage_When_Status_Is_Recommendation_And_All_Valid()
    {
        Setup_Valid_Recommendation();
        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "any-recommendation-slug", false, _controller, default);

        var viewResult = result as ViewResult;

        Assert.NotNull(viewResult);

        var model = viewResult.Model as RecommendationsViewModel;

        Assert.NotNull(model);
        Assert.Equal(_subtopic!.Intros[0], model.Intro);
    }

    [Fact]
    public async Task Should_Show_RecommendationChecklistPage_When_Status_Is_Recommendation_And_All_Valid()
    {
        Setup_Valid_Recommendation();
        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "any-recommendation-slug", true, _controller, default);

        var viewResult = result as ViewResult;

        Assert.NotNull(viewResult);

        var model = viewResult.Model as RecommendationsChecklistViewModel;

        Assert.NotNull(model);
        Assert.Equal(_subtopic!.Intros[0], model.Intro);
        Assert.Contains(AnswerOne, model.Chunks.SelectMany(chunk => chunk.Answers));

        var content = model.AllContent.ToList();
        var intro = content[0];
        var firstChunk = content[1];

        Assert.Equal(_subtopic.Intros[0], intro);
        Assert.Equal("1. test-header", firstChunk.Header.Text);
    }


    private void Setup_Valid_Recommendation()
    {
        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSectionRecommendation(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
            .Do(_ =>
            {
                _submissionStatusProcessor.Status = SubmissionStatus.Completed;
                _submissionStatusProcessor.Section.Returns(_section);
                _submissionStatusProcessor.SectionStatus.Returns(new SectionStatusNew()
                {
                    Maturity = "High"
                });
            });

        _getLatestResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                                .Returns(MockValidLatestResponse);

        _getSubTopicRecommendationQuery.GetSubTopicRecommendation(Arg.Any<string>()).Returns(_subtopic);
    }

    private static SubmissionResponsesDto MockValidLatestResponse(CallInfo callinfo)
    {
        var establishmentId = callinfo.ArgAt<int>(0);
        var subtopicId = callinfo.ArgAt<string>(1);

        return new SubmissionResponsesDto()
        {
            SubmissionId = 1234,
            Responses = [
                new QuestionWithAnswer()
                {
                    AnswerText = AnswerOne.Text,
                    AnswerRef = AnswerOne.Sys.Id,
                    QuestionRef = Question.Sys.Id,
                    QuestionSlug = Question.Slug,
                    QuestionText = Question.Text
                },
            ]
        };
    }
}
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
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
using Answer = Dfe.PlanTech.Domain.Answers.Models.Answer;

namespace Dfe.PlanTech.Web.UnitTests.Routing;

public class GetRecommendationRouterTests
{
    private readonly IGetPageQuery _getPageQuery;
    private readonly ISubmissionStatusProcessor _submissionStatusProcessor;

    private readonly RecommendationsController _controller;
    private readonly GetRecommendationRouter _router;

    private readonly IGetAllAnswersForLatestSubmissionQuery _getAllAnswersForSubmissionQuery;
    private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery;

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
    };

    private readonly SubtopicRecommendation? _subtopic = new SubtopicRecommendation()
    {
        Intros = new List<RecommendationIntro>()
        {
            new RecommendationIntro()
            {
                Slug = "intro-slug",
                Maturity = "High",
            }
        },
        Section = new RecommendationSection()
        {
            Chunks = new List<RecommendationChunk>()
            {
                new RecommendationChunk()
                {
                    Answers = new List<Domain.Questionnaire.Models.Answer>()
                    {
                        new Domain.Questionnaire.Models.Answer()
                        {
                            Sys = new SystemDetails()
                            {
                                Id = "ref1"
                            }
                        }
                    }
                }
            }
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
        _getPageQuery = Substitute.For<IGetPageQuery>();
        _submissionStatusProcessor = Substitute.For<ISubmissionStatusProcessor>();
        _getAllAnswersForSubmissionQuery = Substitute.For<IGetAllAnswersForLatestSubmissionQuery>();
        _getSubTopicRecommendationQuery = Substitute.For<IGetSubTopicRecommendationQuery>();

        _controller = new RecommendationsController(new NullLogger<RecommendationsController>());

        _router = new GetRecommendationRouter(_submissionStatusProcessor, _getAllAnswersForSubmissionQuery, _getSubTopicRecommendationQuery);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Should_ThrowException_When_SectionSlug_NullOrEmpty(string? sectionSlug)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() =>
            _router.ValidateRoute(sectionSlug!, "recommendation-slug", _controller, default));
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Should_ThrowException_When_RecommendationSlug_NullOrEmpty(string? recommendationSlug)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() =>
            _router.ValidateRoute("section-slug", recommendationSlug!, _controller, default));
    }

    [Fact]
    public async Task Should_Redirect_To_CheckAnswersPage_When_Status_CheckAnswers()
    {
        _submissionStatusProcessor.When(processor =>
                processor.GetJourneyStatusForSectionRecommendation(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
            .Do((callinfo) => { _submissionStatusProcessor.Status = SubmissionStatus.CheckAnswers; });

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "recommendation-slug", _controller,
            default);

        var redirectResult = result as RedirectToActionResult;

        Assert.NotNull(redirectResult);

        Assert.Equal(CheckAnswersController.ControllerName, redirectResult.ControllerName);
        Assert.Equal(CheckAnswersController.CheckAnswersAction, redirectResult.ActionName);

        var section = redirectResult.RouteValues?["sectionSlug"];

        Assert.NotNull(section);
        Assert.Equal(_section.InterstitialPage.Slug, section);
    }

    [Fact]
    public async Task Should_Redirect_To_QuestionBySlug_When_Status_NextQuestion()
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

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "recommendation-slug", _controller,
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

    [Fact]
    public async Task Should_Redirect_To_InterstitialPage_When_NotStarted()
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

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "recommendation-slug", _controller,
            default);

        var redirectResult = result as RedirectToActionResult;

        Assert.NotNull(redirectResult);

        Assert.Equal(PagesController.ControllerName, redirectResult.ControllerName);
        Assert.Equal(PagesController.GetPageByRouteAction, PagesController.GetPageByRouteAction);

        var route = redirectResult.RouteValues?["route"];
        Assert.NotNull(route);
        Assert.Equal(PageRedirecter.SelfAssessmentRoute, route);
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Maturity_Null()
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
            _router.ValidateRoute(_section.InterstitialPage.Slug, "recommendation-slug", _controller, default));
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Recommendation_Not_In_Section()
    {
        _submissionStatusProcessor.When(processor =>
                processor.GetJourneyStatusForSectionRecommendation(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
            .Do((callinfo) =>
            {
                _submissionStatusProcessor.Status = SubmissionStatus.Completed;
                _submissionStatusProcessor.Section.Returns(_section);
                _submissionStatusProcessor.SectionStatus.Returns(new SectionStatusNew()
                {
                    Maturity = "High"
                });
            });

        _getAllAnswersForSubmissionQuery.GetAllAnswersForLatestSubmission(Arg.Any<string>(), Arg.Any<int>()).Returns(
            new List<Answer>()
            {
                new Answer()
                {
                    Id = 1,
                    AnswerText = "Answer 1",
                    ContentfulRef = "ref1"
                }
            });

        await Assert.ThrowsAnyAsync<ContentfulDataUnavailableException>(() =>
            _router.ValidateRoute(_section.InterstitialPage.Slug, "other-recommendation-slug", _controller, default));
    }

    [Fact]
    public async Task Should_Throw_Exception_When_NotFind_Recommendation_For_Maturity()
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

        _getAllAnswersForSubmissionQuery.GetAllAnswersForLatestSubmission(Arg.Any<string>(), Arg.Any<int>()).Returns(
            new List<Answer>()
            {
                new Answer()
                {
                    Id = 1,
                    AnswerText = "Answer 1",
                    ContentfulRef = "ref1"
                }
            });


        _getSubTopicRecommendationQuery.GetSubTopicRecommendation(Arg.Any<string>()).Returns(_subtopic);

        await Assert.ThrowsAnyAsync<ContentfulDataUnavailableException>(() =>
            _router.ValidateRoute(_section.InterstitialPage.Slug, "any-recommendation-slug", _controller,
                default));
    }

    [Fact]
    public async Task Should_Show_RecommendationPage_When_Status_Is_Recommendation_And_All_Valid()
    {
        _submissionStatusProcessor.When(processor =>
                processor.GetJourneyStatusForSectionRecommendation(_section.InterstitialPage.Slug, Arg.Any<CancellationToken>()))
            .Do((callinfo) =>
            {
                _submissionStatusProcessor.Status = SubmissionStatus.Completed;
                _submissionStatusProcessor.Section.Returns(_section);
                _submissionStatusProcessor.SectionStatus.Returns(new SectionStatusNew()
                {
                    Maturity = "High"
                });
            });

        _getAllAnswersForSubmissionQuery.GetAllAnswersForLatestSubmission(Arg.Any<string>(), Arg.Any<int>()).Returns(
            new List<Answer>()
            {
                new Answer()
                {
                    Id = 1,
                    AnswerText = "Answer 1",
                    ContentfulRef = "ref1"
                }
            });


        _getSubTopicRecommendationQuery.GetSubTopicRecommendation(Arg.Any<string>()).Returns(_subtopic);

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "any-recommendation-slug",
            _controller, default);

        var viewResult = result as ViewResult;

        Assert.NotNull(viewResult);

        var model = viewResult.Model as RecommendationsViewModel;

        Assert.NotNull(model);
        Assert.Equal(_subtopic!.Intros[0], model.Intro);
    }
}
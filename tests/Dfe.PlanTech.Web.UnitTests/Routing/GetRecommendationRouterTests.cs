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

    private static Answer AnswerOne = new()
    {
        Sys = new()
        {
            Id = "Answer-1"
        },
        Text = "Answer-1-Text",
        NextQuestion = QuestionTwo,
    };

    private static Answer AnswerTwo = new()
    {
        Sys = new()
        {
            Id = "Answer-2"
        },
        Text = "Answer-2-Text",
        NextQuestion = QuestionTwo,
    };

    private static Answer AnswerThree = new()
    {
        Sys = new()
        {
            Id = "Answer-3"
        },
        Text = "Answer-3-Text",
        NextQuestion = QuestionFour,
    };

    private static Answer AnswerFour = new()
    {
        Sys = new()
        {
            Id = "Answer-4"
        },
        Text = "Answer-4-Text",
    };

    private static Answer AnswerFive = new()
    {
        Sys = new()
        {
            Id = "Answer-5"
        },
        Text = "Answer-5-Text",
    };

    private static readonly Question QuestionOne = new()
    {
        Sys = new()
        {
            Id = "Question-1"
        },
        Text = "Question-Text",
        Slug = "Question-1-Slug",
        Answers = []
    };

    private static readonly Question QuestionTwo = new()
    {
        Sys = new()
        {
            Id = "Question-2"
        },
        Text = "Question-2-Text",
        Slug = "Question-2-Slug",
        Answers = []
    };

    private static readonly Question QuestionThree = new()
    {
        Sys = new()
        {
            Id = "Question-3"
        },
        Text = "Question-3-Text",
        Slug = "Question-3-Slug",
        Answers = []
    };

    private static readonly Question QuestionFour = new()
    {
        Sys = new()
        {
            Id = "Question-4"
        },
        Text = "Question-4-Text",
        Slug = "Question-4-Slug",
        Answers = []
    };

    private static readonly Section _section = new()
    {
        InterstitialPage = new Page()
        {
            Slug = "section-slug"
        },
        Sys = new SystemDetails()
        {
            Id = "section-id"
        },
        Questions = [QuestionOne, QuestionTwo, QuestionThree, QuestionFour]
    };

    private readonly SubtopicRecommendation? _subtopicRecommendation = new()
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
                        Text = "test-header-1"
                    },
                    Answers = [AnswerOne]
                },
                new()
                {
                    Header = new()
                    {
                        Text = "test-header-2"
                    },
                    Answers = [AnswerTwo]
                },
                new()
                {
                    Header = new()
                    {
                        Text = "test-header-3"
                    },
                    Answers = [AnswerThree]
                },
                new()
                {
                    Header = new()
                    {
                        Text = "test-header-4"
                    },
                    Answers = [AnswerFour]
                },
                new()
                {
                    Header = new()
                    {
                        Text = "test-header-5"
                    },
                    Answers = [AnswerFive]
                }
            ]
        },
        Subtopic = _section
    };

    public GetRecommendationRouterTests()
    {
        //The NextQuestions were AWOL on answers so... recreating
        AnswerOne = new Answer()
        {
            Sys = AnswerOne.Sys,
            Text = AnswerOne.Text,
            NextQuestion = QuestionTwo
        };

        AnswerTwo = new Answer()
        {
            Sys = AnswerTwo.Sys,
            Text = AnswerTwo.Text,
            NextQuestion = QuestionTwo
        };

        AnswerThree = new Answer()
        {
            Sys = AnswerThree.Sys,
            Text = AnswerThree.Text,
            NextQuestion = QuestionFour
        };

        AnswerFour = new Answer()
        {
            Sys = AnswerFour.Sys,
            Text = AnswerFour.Text,
        };

        AnswerFive = new Answer()
        {
            Sys = AnswerThree.Sys,
            Text = AnswerThree.Text,
        };


        QuestionOne.Answers.Add(AnswerOne);
        QuestionOne.Answers.Add(AnswerTwo);
        QuestionTwo.Answers.Add(AnswerThree);
        QuestionThree.Answers.Add(AnswerFour);
        QuestionFour.Answers.Add(AnswerFive);

        _subtopicRecommendation.Section.Chunks[0].Answers.Add(AnswerOne);
        _subtopicRecommendation.Section.Chunks[1].Answers.Add(AnswerTwo);
        _subtopicRecommendation.Section.Chunks[2].Answers.Add(AnswerThree);
        _subtopicRecommendation.Section.Chunks[3].Answers.Add(AnswerFour);
        _subtopicRecommendation.Section.Chunks[4].Answers.Add(AnswerFive);

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
                                .Returns((callinfo) => MockValidLatestResponse(callinfo));

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
                                .Returns((callinfo) => MockValidLatestResponse(callinfo));

        _getSubTopicRecommendationQuery.GetSubTopicRecommendation(Arg.Any<string>()).Returns(_subtopicRecommendation);

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
        Assert.Equal(_subtopicRecommendation!.Intros[0], model.Intro);
    }

    [Fact]
    public async Task Should_Return_Only_Last_Recommendation_Journey()
    {
        List<QuestionWithAnswer> responses = [
            new QuestionWithAnswer()
            {
                AnswerText = AnswerOne.Text,
                AnswerRef = AnswerOne.Sys.Id,
                QuestionRef = QuestionOne.Sys.Id,
                QuestionSlug = QuestionOne.Slug,
                QuestionText = QuestionOne.Text
            },
            new QuestionWithAnswer()
            {
                AnswerText = AnswerThree.Text,
                AnswerRef = AnswerThree.Sys.Id,
                QuestionRef = QuestionTwo.Sys.Id,
                QuestionSlug = QuestionTwo.Slug,
                QuestionText = QuestionTwo.Text
            },
            new QuestionWithAnswer()
            {
                AnswerText = AnswerFour.Text,
                AnswerRef = AnswerFour.Sys.Id,
                QuestionRef = QuestionThree.Sys.Id,
                QuestionSlug = QuestionThree.Slug,
                QuestionText = QuestionThree.Text
            },
            new QuestionWithAnswer()
            {
                AnswerText = AnswerFive.Text,
                AnswerRef = AnswerFive.Sys.Id,
                QuestionRef = QuestionFour.Sys.Id,
                QuestionSlug = QuestionFour.Slug,
                QuestionText = QuestionFour.Text
            },
        ];

        //Expected route = Q1 - A1 -> Q2 + A3 -> Q4 + A5

        Setup_Valid_Recommendation(responses);

        var result = await _router.ValidateRoute(_section.InterstitialPage.Slug, "any-recommendation-slug", true, _controller, default);

        var viewResult = result as ViewResult;

        Assert.NotNull(viewResult);

        var model = viewResult.Model as RecommendationsChecklistViewModel;

        Assert.NotNull(model);
        Assert.Equal(_subtopicRecommendation!.Intros[0], model.Intro);

        Assert.Contains(AnswerOne, model.Chunks.SelectMany(chunk => chunk.Answers));
        Assert.Contains(AnswerThree, model.Chunks.SelectMany(chunk => chunk.Answers));
        Assert.Contains(AnswerFive, model.Chunks.SelectMany(chunk => chunk.Answers));

        Assert.Equal(3, model.Chunks.Count);
    }

    private void Setup_Valid_Recommendation(List<QuestionWithAnswer>? responses = null)
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
                                .Returns((callinfo) => MockValidLatestResponse(callinfo, responses));

        _getSubTopicRecommendationQuery.GetSubTopicRecommendation(Arg.Any<string>()).Returns(_subtopicRecommendation);
    }

    private static SubmissionResponsesDto MockValidLatestResponse(CallInfo callinfo, List<QuestionWithAnswer>? responses = null)
    {
        var establishmentId = callinfo.ArgAt<int>(0);
        var subtopicId = callinfo.ArgAt<string>(1);

        return new SubmissionResponsesDto()
        {
            SubmissionId = 1234,
            Responses = responses ?? [
                new QuestionWithAnswer()
                {
                    AnswerText = AnswerOne.Text,
                    AnswerRef = AnswerOne.Sys.Id,
                    QuestionRef = QuestionOne.Sys.Id,
                    QuestionSlug = QuestionOne.Slug,
                    QuestionText = QuestionOne.Text
                },
            ]
        };
    }
}
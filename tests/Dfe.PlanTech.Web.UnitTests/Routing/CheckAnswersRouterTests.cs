using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
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

namespace Dfe.PlanTech.Web.UnitTests.Routing;

public class CheckAnswersRouterTests
{
    private readonly IGetPageQuery _getPageQuery = Substitute.For<IGetPageQuery>();
    private readonly IProcessSubmissionResponsesCommand _checkAnswerCommand = Substitute.For<IProcessSubmissionResponsesCommand>();
    private readonly IUser _user = Substitute.For<IUser>();
    private readonly ISubmissionStatusProcessor _submissionStatusProcessor = Substitute.For<ISubmissionStatusProcessor>();

    private readonly CheckAnswersController _controller = new(new NullLogger<CheckAnswersController>());

    private readonly CheckAnswersRouter _router;

    private const int establishmentId = 1;

    private readonly Section _section = new()
    {
        Sys = new SystemDetails()
        {
            Id = "section-id"
        },
        InterstitialPage = new Page()
        {
            Slug = "section-slug"
        },
        Name = "Section name"
    };

    private readonly SubmissionResponsesDto _checkAnswersDto = new()
    {
        Responses = [
            new()
            {
                QuestionRef = "q1",
                AnswerRef = "a1"
            },
            new()
            {
                QuestionRef = "q2",
                AnswerRef = "q2-a1"
            }
        ]
    };

    private readonly List<ContentComponent> _checkAnswersPageContent = new(){
        new Title(){Text = "page title" }
      };

    public CheckAnswersRouterTests()
    {
        _user.GetEstablishmentId().Returns(establishmentId);
        _checkAnswerCommand.GetSubmissionResponsesDtoForSection(establishmentId, Arg.Any<Section>(), Arg.Any<CancellationToken>())
                            .Returns((callinfo) =>
                            {
                                var sectionArg = callinfo.ArgAt<ISection>(1);
                                if (sectionArg == _section)
                                {
                                    return _checkAnswersDto;
                                }

                                return null;
                            });

        _getPageQuery.GetPageBySlug(CheckAnswersController.CheckAnswersPageSlug, Arg.Any<CancellationToken>())
                    .Returns(new Page()
                    {
                        Slug = CheckAnswersController.CheckAnswersPageSlug,
                        Content = _checkAnswersPageContent
                    });

        _router = new CheckAnswersRouter(_getPageQuery, _checkAnswerCommand, _user, _submissionStatusProcessor);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Should_Throw_Exception_When_SectionSlug_NullOrEmpty(string? sectionSlug)
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _router.ValidateRoute(sectionSlug!, null, _controller, default));
    }

    [Fact]
    public async Task Should_Redirect_To_NextQuestion_When_Status_InProgresss()
    {
        var sectionSlug = "section-slug";

        var nextQuestion = new Question()
        {
            Slug = "next-question"
        };

        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(sectionSlug, Arg.Any<CancellationToken>()))
        .Do(callInfo =>
        {
            _submissionStatusProcessor.Status = Status.InProgress;
            _submissionStatusProcessor.NextQuestion = nextQuestion;
        });

        var result = await _router.ValidateRoute(sectionSlug, null, _controller, default);

        var redirectResult = result as RedirectToActionResult;

        Assert.NotNull(redirectResult);
        Assert.Equal(QuestionsController.Controller, redirectResult.ControllerName);
        Assert.Equal(QuestionsController.GetQuestionBySlugActionName, redirectResult.ActionName);

        var sectionSlugValue = redirectResult.RouteValues?["sectionSlug"];
        Assert.NotNull(sectionSlugValue);
        Assert.Equal(sectionSlug, sectionSlugValue);

        var questionSlugValue = redirectResult.RouteValues?["questionSlug"];
        Assert.NotNull(questionSlugValue);
        Assert.Equal(nextQuestion.Slug, questionSlugValue);
    }

    [Fact]
    public async Task Should_Return_CheckAnswersPage_When_Status_Is_CheckAnswers()
    {
        var sectionSlug = _section.InterstitialPage?.Slug ?? throw new InvalidOperationException("InterstitialPage cannot be null.");

        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(sectionSlug, Arg.Any<CancellationToken>()))
                                  .Do(processor =>
                                  {
                                      _submissionStatusProcessor.Status = Status.CompleteNotReviewed;
                                      _submissionStatusProcessor.Section.Returns(_section);
                                  });

        var result = await _router.ValidateRoute(sectionSlug, null, _controller, default);

        var pageResult = result as ViewResult;

        Assert.NotNull(pageResult);

        var model = pageResult.Model as CheckAnswersViewModel;

        Assert.NotNull(model);

        Assert.Equal(_section.Name, model.SectionName);
        Assert.Equal(sectionSlug, model.SectionSlug);
        Assert.Equal(_checkAnswersDto, model.SubmissionResponses);
        Assert.Equal(_checkAnswersDto.SubmissionId, model.SubmissionId);
        Assert.Equal(_checkAnswersPageContent, model.Content);
    }

    [Fact]
    public async Task Should_Throw_DatabaseException_When_Responses_Null()
    {
        var noneAnsweredSection = new Section()
        {
            Sys = new SystemDetails()
            {
                Id = "non-answered-section"
            },
            InterstitialPage = new Page()
            {
                Slug = "non-answered-section-slug"
            }
        };

        var sectionSlug = noneAnsweredSection.InterstitialPage.Slug;

        _submissionStatusProcessor.When(processor => processor.GetJourneyStatusForSection(sectionSlug, Arg.Any<CancellationToken>()))
                                  .Do(processor =>
                                  {
                                      _submissionStatusProcessor.Status = Status.CompleteNotReviewed;
                                      _submissionStatusProcessor.Section.Returns(noneAnsweredSection);
                                  });

        await Assert.ThrowsAnyAsync<DatabaseException>(() => _router.ValidateRoute(sectionSlug, null, _controller, default));
    }
}

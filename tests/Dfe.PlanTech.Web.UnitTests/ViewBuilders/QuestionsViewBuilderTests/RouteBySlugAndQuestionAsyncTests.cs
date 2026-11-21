using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.QuestionsViewBuilderTests;

public class RouteBySlugAndQuestionAsyncTests : QuestionsViewBuilderTestBase
{
    private const string ReturnTo = "check-answers";
    private const string AnswerId = "answer-1-id";

    private readonly SubmissionResponsesModel _submissionAnsweringFirstQuestion = new(
        1,
        [
            new QuestionWithAnswerModel
            {
                QuestionSysId = FirstQuestionId,
                AnswerSysId = AnswerId,
                QuestionText = "Question one",
                AnswerText = "Answer one",
            },
        ]
    );

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenSlugIsForNextQuestion_ReturnsQuestionView()
    {
        // Arrange
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    _firstQuestion,
                    _section,
                    null,
                    SubmissionStatus.InProgress
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySlugAndQuestionAsync(
            _controller,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Question", view.ViewName);
        var model = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.Same(_firstQuestion, model.Question);
        Assert.Null(model.AnswerSysId);
        Assert.Equal(SectionName, model.SectionName);
        Assert.Equal(SectionId, model.SectionId);
        Assert.Equal(CategorySlug, model.CategorySlug);
        Assert.Equal(SectionSlug, model.SectionSlug);
        Assert.Equal(
            _firstQuestion.Text,
            _controller.ViewData[StatePassingMechanismConstants.Title]
        );
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenUserCannotSelectSchools_UsesFocusedEstablishmentAndDoesNotSetSelectedSchoolNames()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(false);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    _firstQuestion,
                    _section,
                    null,
                    SubmissionStatus.InProgress
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySlugAndQuestionAsync(
            _controller,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.Empty(model.SelectedSchoolNames);
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenUserCanSelectSchools_UsesFirstSelectedSchoolAndSetsSelectedSchoolNames()
    {
        // Arrange
        List<string> expectedSchoolNames = ["School One", "School Two"];
        var firstSelectedSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(101)
            .Build();
        var secondSelectedSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(102)
            .Build();
        _currentUserProvider.CanSelectSchools.Returns(true);
        _currentUserProvider
            .GetSelectedSchools()
            .Returns([firstSelectedSchool, secondSelectedSchool]);
        _currentUserProvider.GetSelectedSchoolNames().Returns(expectedSchoolNames);
        _submissionService
            .GetSubmissionRoutingDataAsync(101, _section, SubmissionStatus.InProgress)
            .Returns(
                new SubmissionRoutingDataModel(
                    _firstQuestion,
                    _section,
                    null,
                    SubmissionStatus.InProgress
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySlugAndQuestionAsync(
            _controller,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.Same(_firstQuestion, model.Question);
        Assert.Equal(expectedSchoolNames, model.SelectedSchoolNames);
        await _currentUserProvider.DidNotReceive().GetFocusedEstablishmentAsync();
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync("missing-section")
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteBySlugAndQuestionAsync(
                _controller,
                CategorySlug,
                "missing-section",
                FirstQuestionSlug,
                null
            )
        );
        Assert.Contains("missing-section", exception.Message);
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenSubmissionNotStarted_RedirectsToInterstitialPage()
    {
        // Arrange
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(
                new SubmissionRoutingDataModel(null, _section, null, SubmissionStatus.NotStarted)
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySlugAndQuestionAsync(
            _controller,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
        Assert.Equal(nameof(PagesController).GetControllerNameSlug(), redirect.ControllerName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(SectionSlug, redirect.RouteValues["route"]);
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenSubmissionHasNoResponses_ThrowsInvalidOperationException()
    {
        // Arrange
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(
                new SubmissionRoutingDataModel(null, _section, null, SubmissionStatus.InProgress)
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.RouteBySlugAndQuestionAsync(
                _controller,
                CategorySlug,
                SectionSlug,
                FirstQuestionSlug,
                null
            )
        );
        Assert.Contains(SectionId, exception.Message);
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenQuestionSlugIsNotInSection_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    _secondQuestion,
                    _section,
                    _submissionAnsweringFirstQuestion,
                    SubmissionStatus.InProgress
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteBySlugAndQuestionAsync(
                _controller,
                CategorySlug,
                SectionSlug,
                "unknown-question-slug",
                null
            )
        );
        Assert.Contains("unknown-question-slug", exception.Message);
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenQuestionHasBeenAnswered_ReturnsQuestionViewWithLatestAnswer()
    {
        // Arrange
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    _secondQuestion,
                    _section,
                    _submissionAnsweringFirstQuestion,
                    SubmissionStatus.InProgress
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySlugAndQuestionAsync(
            _controller,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Question", view.ViewName);
        var model = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.Same(_firstQuestion, model.Question);
        Assert.Equal(AnswerId, model.AnswerSysId);
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenQuestionNotAnsweredAndSubmissionInProgress_ReturnsNextQuestionView()
    {
        // Arrange
        var routingData = new SubmissionRoutingDataModel(
            _secondQuestion,
            _section,
            new SubmissionResponsesModel(1, []),
            SubmissionStatus.InProgress
        );
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(routingData);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySlugAndQuestionAsync(
            _controller,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.Same(_secondQuestion, model.Question);
        Assert.Null(model.AnswerSysId);
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenQuestionNotAnsweredAndNoNextQuestionInProgress_ThrowsInvalidDataException()
    {
        // Arrange
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    _section,
                    new SubmissionResponsesModel(1, []),
                    SubmissionStatus.InProgress
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.RouteBySlugAndQuestionAsync(
                _controller,
                CategorySlug,
                SectionSlug,
                FirstQuestionSlug,
                null
            )
        );
        Assert.Equal("NextQuestion is null", exception.Message);
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenQuestionNotAnsweredAndSubmissionComplete_RedirectsToCheckAnswers()
    {
        // Arrange
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    _section,
                    new SubmissionResponsesModel(1, []),
                    SubmissionStatus.CompleteNotReviewed
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySlugAndQuestionAsync(
            _controller,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ReviewAnswersController.CheckAnswers), redirect.ActionName);
        Assert.Equal(
            nameof(ReviewAnswersController).GetControllerNameSlug(),
            redirect.ControllerName
        );
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(CategorySlug, redirect.RouteValues["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenReturnToIsProvided_SetsReturnToViewData()
    {
        // Arrange
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    _firstQuestion,
                    _section,
                    null,
                    SubmissionStatus.InProgress
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteBySlugAndQuestionAsync(
            _controller,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            ReturnTo
        );

        // Assert
        Assert.Equal(ReturnTo, _controller.ViewData[StatePassingMechanismConstants.ReturnTo]);
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenReturnToIsNotProvided_DoesNotSetReturnToViewData()
    {
        // Arrange
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    _firstQuestion,
                    _section,
                    null,
                    SubmissionStatus.InProgress
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteBySlugAndQuestionAsync(
            _controller,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        Assert.False(_controller.ViewData.ContainsKey(StatePassingMechanismConstants.ReturnTo));
    }

    [Fact]
    public async Task RouteBySlugAndQuestionAsync_WhenAnswerLeadsToAnotherQuestion_ClearsAnswersOfNextQuestion()
    {
        // Arrange
        var nestedQuestion = new QuestionnaireQuestionEntry
        {
            Sys = new SystemDetails("nested-question-id"),
            Slug = "nested-question-slug",
            Answers = [new QuestionnaireAnswerEntry()],
        };
        var question = new QuestionnaireQuestionEntry
        {
            Sys = new SystemDetails("parent-question-id"),
            Slug = "parent-question-slug",
            Answers =
            [
                new QuestionnaireAnswerEntry { NextQuestion = nestedQuestion },
                new QuestionnaireAnswerEntry { NextQuestion = null },
            ],
        };
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    question,
                    _section,
                    null,
                    SubmissionStatus.InProgress
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteBySlugAndQuestionAsync(
            _controller,
            CategorySlug,
            SectionSlug,
            "parent-question-slug",
            null
        );

        // Assert
        Assert.Empty(nestedQuestion.Answers);
    }
}

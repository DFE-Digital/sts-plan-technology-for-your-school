using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.QuestionsViewBuilderTests;

public class SubmitAnswerAndRedirectTests : QuestionsViewBuilderTestBase
{
    private const int UserId = 11;
    private const int UserEstablishmentId = 33;
    private const string AnswerId = "answer-1-id";
    private const string ReturnTo = "check-answers";

    private readonly SubmitAnswerInputViewModel _answerInput = new()
    {
        SectionId = SectionId,
        SectionName = SectionName,
        QuestionId = FirstQuestionId,
        QuestionText = "Question one",
        ChosenAnswerJson = $$"""{"Id":"{{AnswerId}}","Text":"Answer one"}""",
    };

    public SubmitAnswerAndRedirectTests()
    {
        _currentUserProvider.UserId.Returns(UserId);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(UserEstablishmentId).Build());
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_WhenUserCannotSelectSchools_SubmitsAnswerForFocusedEstablishment()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(false);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SubmitAnswerAndRedirect(
            _controller,
            _answerInput,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        await _submissionService
            .Received(1)
            .SubmitAnswerAsync(
                UserId,
                FocusedEstablishmentId,
                UserEstablishmentId,
                Arg.Is<SubmitAnswerModel>(model =>
                    model.SectionId == SectionId
                    && model.Question.Id == FirstQuestionId
                    && model.ChosenAnswer!.Id == AnswerId
                )
            );
        await _submissionService
            .ReceivedWithAnyArgs(1)
            .SubmitAnswerAsync(default, default, default, default!);
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_WhenUserCanSelectSchools_SubmitsAnswerForEachSelectedSchool()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(true);
        _currentUserProvider.GetSelectedSchoolEstablishmentIds().Returns([101, 102, 103]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SubmitAnswerAndRedirect(
            _controller,
            _answerInput,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        await _submissionService
            .Received(1)
            .SubmitAnswerAsync(UserId, 101, UserEstablishmentId, Arg.Any<SubmitAnswerModel>());
        await _submissionService
            .Received(1)
            .SubmitAnswerAsync(UserId, 102, UserEstablishmentId, Arg.Any<SubmitAnswerModel>());
        await _submissionService
            .Received(1)
            .SubmitAnswerAsync(UserId, 103, UserEstablishmentId, Arg.Any<SubmitAnswerModel>());
        await _submissionService
            .ReceivedWithAnyArgs(3)
            .SubmitAnswerAsync(default, default, default, default!);
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_WhenUserCanSelectSchoolsButNoneAreSelected_ThrowsInvalidOperationException()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(true);
        _currentUserProvider.GetSelectedSchoolEstablishmentIds().Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.SubmitAnswerAndRedirect(
                _controller,
                _answerInput,
                CategorySlug,
                SectionSlug,
                FirstQuestionSlug,
                null
            )
        );
        Assert.Contains("no focused or selected schools", exception.Message);
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync("missing-section")
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.SubmitAnswerAndRedirect(
                _controller,
                _answerInput,
                CategorySlug,
                "missing-section",
                FirstQuestionSlug,
                null
            )
        );
        Assert.Contains("missing-section", exception.Message);
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_WhenModelStateIsInvalid_ReturnsQuestionViewWithErrorMessages()
    {
        // Arrange
        _controller.ModelState.AddModelError(
            nameof(SubmitAnswerInputViewModel.ChosenAnswerJson),
            "You must select an answer to continue"
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SubmitAnswerAndRedirect(
            _controller,
            _answerInput,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            ReturnTo
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Question", view.ViewName);
        var model = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.Same(_firstQuestion, model.Question);
        Assert.Equal(AnswerId, model.AnswerSysId);
        Assert.Equal(SectionName, model.SectionName);
        Assert.Equal(SectionId, model.SectionId);
        Assert.Equal(CategorySlug, model.CategorySlug);
        Assert.Equal(SectionSlug, model.SectionSlug);
        var errorMessage = Assert.Single(model.ErrorMessages!);
        Assert.Equal("You must select an answer to continue", errorMessage);
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_WhenModelStateIsInvalid_DoesNotSubmitAnswer()
    {
        // Arrange
        _controller.ModelState.AddModelError(
            nameof(SubmitAnswerInputViewModel.ChosenAnswerJson),
            "You must select an answer to continue"
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SubmitAnswerAndRedirect(
            _controller,
            new SubmitAnswerInputViewModel(),
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        await _submissionService
            .DidNotReceiveWithAnyArgs()
            .SubmitAnswerAsync(default, default, default, default!);
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_WhenSubmittingAnswerFails_ReturnsQuestionViewWithSaveFailedMessage()
    {
        // Arrange
        _submissionService
            .SubmitAnswerAsync(
                UserId,
                FocusedEstablishmentId,
                UserEstablishmentId,
                Arg.Any<SubmitAnswerModel>()
            )
            .ThrowsAsync(new InvalidOperationException("database-unavailable"));
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SubmitAnswerAndRedirect(
            _controller,
            _answerInput,
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
        var errorMessage = Assert.Single(model.ErrorMessages!);
        Assert.Equal("Save failed. Please try again later.", errorMessage);
        await _questionService
            .DidNotReceiveWithAnyArgs()
            .GetNextUnansweredQuestion(default, default!);
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_WhenSubmittingAnswerFails_LogsError()
    {
        // Arrange
        _submissionService
            .SubmitAnswerAsync(
                UserId,
                FocusedEstablishmentId,
                UserEstablishmentId,
                Arg.Any<SubmitAnswerModel>()
            )
            .ThrowsAsync(new InvalidOperationException("database-unavailable"));
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SubmitAnswerAndRedirect(
            _controller,
            _answerInput,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot(), record =>
            record.Level == LogLevel.Error
        );
        Assert.Contains("database-unavailable", logRecord.Message);
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_WhenNextQuestionExists_RedirectsToNextQuestion()
    {
        // Arrange
        _questionService
            .GetNextUnansweredQuestion(FocusedEstablishmentId, _section)
            .Returns(_secondQuestion);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SubmitAnswerAndRedirect(
            _controller,
            _answerInput,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            ReturnTo
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.GetQuestionBySlug), redirect.ActionName);
        Assert.Equal(QuestionsController.Controller, redirect.ControllerName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(CategorySlug, redirect.RouteValues["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
        Assert.Equal(SecondQuestionSlug, redirect.RouteValues["questionSlug"]);
        Assert.Equal(ReturnTo, redirect.RouteValues["returnTo"]);
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_WhenNoQuestionIsLeftUnanswered_RedirectsToCheckAnswers()
    {
        // Arrange
        _questionService
            .GetNextUnansweredQuestion(FocusedEstablishmentId, _section)
            .Returns((QuestionnaireQuestionEntry?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SubmitAnswerAndRedirect(
            _controller,
            _answerInput,
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
    public async Task SubmitAnswerAndRedirect_WhenUserCanSelectSchools_RedirectsUsingNextQuestionOfFirstSelectedSchool()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(true);
        _currentUserProvider.GetSelectedSchoolEstablishmentIds().Returns([101, 102]);
        _questionService.GetNextUnansweredQuestion(101, _section).Returns(_secondQuestion);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SubmitAnswerAndRedirect(
            _controller,
            _answerInput,
            CategorySlug,
            SectionSlug,
            FirstQuestionSlug,
            null
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.GetQuestionBySlug), redirect.ActionName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(SecondQuestionSlug, redirect.RouteValues["questionSlug"]);
    }
}

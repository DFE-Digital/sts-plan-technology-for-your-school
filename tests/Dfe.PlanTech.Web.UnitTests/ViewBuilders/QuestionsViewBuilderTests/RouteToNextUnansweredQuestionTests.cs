using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.QuestionsViewBuilderTests;

public class RouteToNextUnansweredQuestionTests : QuestionsViewBuilderTestBase
{
    private const string ContactHref = "https://example.org/contact";

    [Fact]
    public async Task RouteToNextUnansweredQuestion_WhenNextQuestionExists_RedirectsToThatQuestion()
    {
        // Arrange
        _questionService
            .GetNextUnansweredQuestion(FocusedEstablishmentId, _section)
            .Returns(_secondQuestion);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToNextUnansweredQuestion(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.GetQuestionBySlug), redirect.ActionName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(CategorySlug, redirect.RouteValues["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
        Assert.Equal(SecondQuestionSlug, redirect.RouteValues["questionSlug"]);
    }

    [Fact]
    public async Task RouteToNextUnansweredQuestion_WhenNoQuestionIsUnanswered_RedirectsToCheckAnswers()
    {
        // Arrange
        _questionService
            .GetNextUnansweredQuestion(FocusedEstablishmentId, _section)
            .Returns((QuestionnaireQuestionEntry?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToNextUnansweredQuestion(
            _controller,
            CategorySlug,
            SectionSlug
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
    public async Task RouteToNextUnansweredQuestion_WhenDatabaseExceptionIsThrown_RedirectsToHomePage()
    {
        // Arrange
        _questionService
            .GetNextUnansweredQuestion(FocusedEstablishmentId, _section)
            .ThrowsAsync(new DatabaseException("database-error"));
        _contentfulService
            .GetLinkByIdAsync(ContactLinkId)
            .Returns(new NavigationLinkEntry { Href = ContactHref });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToNextUnansweredQuestion(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
        Assert.Equal(nameof(PagesController).GetControllerNameSlug(), redirect.ControllerName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(UrlConstants.Home, redirect.RouteValues["route"]);
    }

    [Fact]
    public async Task RouteToNextUnansweredQuestion_WhenDatabaseExceptionIsThrown_MarksSubmissionInaccessible()
    {
        // Arrange
        _questionService
            .GetNextUnansweredQuestion(FocusedEstablishmentId, _section)
            .ThrowsAsync(new DatabaseException("database-error"));
        _contentfulService
            .GetLinkByIdAsync(ContactLinkId)
            .Returns(new NavigationLinkEntry { Href = ContactHref });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToNextUnansweredQuestion(_controller, CategorySlug, SectionSlug);

        // Assert
        await _submissionService
            .Received(1)
            .SetSubmissionInaccessibleAsync(FocusedEstablishmentId, SectionId);
    }

    [Fact]
    public async Task RouteToNextUnansweredQuestion_WhenDatabaseExceptionIsThrownAndContactLinkExists_SetsErrorMessageWithContactLink()
    {
        // Arrange
        _questionService
            .GetNextUnansweredQuestion(FocusedEstablishmentId, _section)
            .ThrowsAsync(new DatabaseException("database-error"));
        _contentfulService
            .GetLinkByIdAsync(ContactLinkId)
            .Returns(new NavigationLinkEntry { Href = ContactHref });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToNextUnansweredQuestion(_controller, CategorySlug, SectionSlug);

        // Assert
        Assert.Equal(
            "There was a problem, please <a href=\"https://example.org/contact\" target=\"_blank\">contact us</a>.",
            _controller.TempData[StatePassingMechanismConstants.SubtopicError]
        );
    }

    [Fact]
    public async Task RouteToNextUnansweredQuestion_WhenDatabaseExceptionIsThrownAndContactLinkHasNoHref_SetsPlainErrorMessage()
    {
        // Arrange
        _questionService
            .GetNextUnansweredQuestion(FocusedEstablishmentId, _section)
            .ThrowsAsync(new DatabaseException("database-error"));
        _contentfulService
            .GetLinkByIdAsync(ContactLinkId)
            .Returns(new NavigationLinkEntry { Href = string.Empty });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToNextUnansweredQuestion(_controller, CategorySlug, SectionSlug);

        // Assert
        Assert.Equal(
            ConcurrentUsersErrorMessage,
            _controller.TempData[StatePassingMechanismConstants.SubtopicError]
        );
    }
}

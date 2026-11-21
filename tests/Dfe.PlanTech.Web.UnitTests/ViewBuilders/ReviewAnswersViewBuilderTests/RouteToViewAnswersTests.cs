using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.ReviewAnswersViewBuilderTests;

public class RouteToViewAnswersTests : ReviewAnswersViewBuilderTestBase
{
    private const string NextQuestionSlug = "next-question-slug";
    private const int SubmissionId = 42;
    private const int SelectedEstablishmentId = 101;

    private static readonly DateTime CompletedDate = new(2025, 3, 20, 10, 30, 0, DateTimeKind.Utc);

    [Fact]
    public async Task RouteToViewAnswers_WhenUserCannotSelectSchools_UsesFocusedEstablishment()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        ArrangeFocusedEstablishment();
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(section);
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.CompleteReviewed,
            SubmissionStatus.CompleteReviewed,
            new SubmissionResponsesModel(SubmissionId, []) { DateCompleted = CompletedDate }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToViewAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug
        );

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ViewAnswersViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task RouteToViewAnswers_WhenUserCanSelectSchools_UsesFirstSelectedSchool()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        _currentUserProvider.CanSelectSchools.Returns(true);
        _currentUserProvider
            .GetSelectedSchools()
            .Returns([
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(SelectedEstablishmentId)
                    .Build(),
            ]);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(section);
        ArrangeRouting(
            SelectedEstablishmentId,
            section,
            SubmissionStatus.CompleteReviewed,
            SubmissionStatus.CompleteReviewed,
            new SubmissionResponsesModel(SubmissionId, []) { DateCompleted = CompletedDate }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToViewAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug
        );

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ViewAnswersViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task RouteToViewAnswers_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeFocusedEstablishment();
        _contentfulService
            .GetSectionBySlugAsync(SectionSlug)
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToViewAnswers(new TestController(), CategorySlug, SectionSlug)
        );
        Assert.Equal($"Could not find section for slug {SectionSlug}", exception.Message);
    }

    [Fact]
    public async Task RouteToViewAnswers_WhenSubmissionIsNotStarted_RedirectsToHomePage()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        ArrangeFocusedEstablishment();
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(section);
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.CompleteReviewed,
            SubmissionStatus.NotStarted
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToViewAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
        Assert.Equal("Pages", redirect.ControllerName);
        Assert.Equal(UrlConstants.HomePage, redirect.RouteValues?["route"]);
    }

    [Theory]
    [InlineData(SubmissionStatus.InProgress)]
    [InlineData(SubmissionStatus.CompleteNotReviewed)]
    public async Task RouteToViewAnswers_WhenSubmissionIsNotReviewed_RedirectsToContinueSelfAssessment(
        SubmissionStatus status
    )
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        ArrangeFocusedEstablishment();
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(section);
        ArrangeRouting(FocusedEstablishmentId, section, SubmissionStatus.CompleteReviewed, status);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToViewAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.GetContinueSelfAssessment), redirect.ActionName);
        Assert.Equal("Questions", redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues?["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues?["sectionSlug"]);
    }

    [Fact]
    public async Task RouteToViewAnswers_WhenSubmissionIsReviewed_ReturnsViewAnswersView()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
            Questions = [new QuestionnaireQuestionEntry { Sys = new SystemDetails("question-id") }],
        };
        var response = new QuestionWithAnswerModel
        {
            QuestionSysId = "question-id",
            AnswerSysId = "answer-id",
        };
        ArrangeFocusedEstablishment();
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(section);
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.CompleteReviewed,
            SubmissionStatus.CompleteReviewed,
            new SubmissionResponsesModel(SubmissionId, [response]) { DateCompleted = CompletedDate }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToViewAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug
        );

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(ReviewAnswersViewBuilder.ViewAnswersViewName, viewResult.ViewName);
        var viewModel = Assert.IsType<ViewAnswersViewModel>(viewResult.Model);
        Assert.Equal(SectionName, viewModel.TopicName);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.Equal(SectionSlug, viewModel.SectionSlug);
        Assert.Equal(CompletedDate, viewModel.AssessmentCompletedDate);
        Assert.Same(response, Assert.Single(viewModel.Responses));
        Assert.False(viewModel.IsMatInProgressView);
        Assert.Equal($"/{CategorySlug}", viewModel.BackLinkHref);
        Assert.Equal("Back to recommendations", viewModel.BackButtonText);
    }

    [Fact]
    public async Task RouteToViewAnswers_WhenReviewedSubmissionIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        ArrangeFocusedEstablishment();
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(section);
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.CompleteReviewed,
            SubmissionStatus.CompleteReviewed
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.RouteToViewAnswers(new TestController(), CategorySlug, SectionSlug)
        );
        Assert.Equal(
            $"Submission cannot be null when status is {SubmissionStatus.CompleteReviewed}",
            exception.Message
        );
    }

    [Theory]
    [InlineData(SubmissionStatus.None)]
    [InlineData(SubmissionStatus.Inaccessible)]
    [InlineData(SubmissionStatus.Obsolete)]
    [InlineData(SubmissionStatus.RetrievalError)]
    public async Task RouteToViewAnswers_WhenStatusIsNotViewable_RedirectsToNextQuestion(
        SubmissionStatus status
    )
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        ArrangeFocusedEstablishment();
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(section);
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.CompleteReviewed,
            status,
            nextQuestion: new QuestionnaireQuestionEntry { Slug = NextQuestionSlug }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToViewAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.GetQuestionBySlug), redirect.ActionName);
        Assert.Equal("Questions", redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues?["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues?["sectionSlug"]);
        Assert.Equal(NextQuestionSlug, redirect.RouteValues?["nextQuestionSlug"]);
    }
}

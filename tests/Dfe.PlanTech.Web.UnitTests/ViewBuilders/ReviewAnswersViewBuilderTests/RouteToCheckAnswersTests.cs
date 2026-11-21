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

public class RouteToCheckAnswersTests : ReviewAnswersViewBuilderTestBase
{
    private const string ErrorMessage = "error-message";
    private const string NextQuestionSlug = "next-question-slug";
    private const int SubmissionId = 42;
    private const int FirstSelectedEstablishmentId = 101;
    private const int SecondSelectedEstablishmentId = 102;

    private void ArrangeCheckAnswersContent(
        QuestionnaireSectionEntry section,
        PageEntry? page = null
    )
    {
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(section);
        _contentfulService
            .GetAllCategoriesAsync()
            .Returns([
                new QuestionnaireCategoryEntry
                {
                    Header = new ComponentHeaderEntry { Text = CategoryName },
                    Sections = [section],
                },
            ]);
        _contentfulService
            .GetPageBySlugAsync(UrlConstants.CheckAnswersSlug)
            .Returns(page ?? new PageEntry { Content = [] });
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenUserCannotSelectSchools_UsesFocusedEstablishment()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        ArrangeFocusedEstablishment();
        ArrangeCheckAnswersContent(section);
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.InProgress,
            SubmissionStatus.InProgress,
            new SubmissionResponsesModel(SubmissionId, [])
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCheckAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug
        );

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ReviewAnswersViewModel>(viewResult.Model);
        Assert.Equal(SubmissionId, viewModel.SubmissionId);
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenUserCanSelectSchools_UsesFirstSelectedSchool()
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
                    .WithEstablishmentId(FirstSelectedEstablishmentId)
                    .Build(),
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(SecondSelectedEstablishmentId)
                    .Build(),
            ]);
        _currentUserProvider.GetSelectedSchoolNames().Returns([]);
        ArrangeCheckAnswersContent(section);
        ArrangeRouting(
            FirstSelectedEstablishmentId,
            section,
            SubmissionStatus.InProgress,
            SubmissionStatus.InProgress,
            new SubmissionResponsesModel(SubmissionId, [])
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCheckAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug
        );

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ReviewAnswersViewModel>(viewResult.Model);
        Assert.Equal(SubmissionId, viewModel.SubmissionId);
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenUserCanSelectSchoolsButNoneSelected_ThrowsInvalidOperationException()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(true);
        _currentUserProvider.GetSelectedSchools().Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.RouteToCheckAnswers(new TestController(), CategorySlug, SectionSlug)
        );
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeFocusedEstablishment();
        _contentfulService
            .GetSectionBySlugAsync(SectionSlug)
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToCheckAnswers(new TestController(), CategorySlug, SectionSlug)
        );
        Assert.Equal($"Could not find section for slug {SectionSlug}", exception.Message);
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenSubmissionIsNotStarted_RedirectsToHomePage()
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
            SubmissionStatus.InProgress,
            SubmissionStatus.NotStarted
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCheckAnswers(
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
    [InlineData(SubmissionStatus.CompleteReviewed)]
    public async Task RouteToCheckAnswers_WhenSubmissionHasAnswers_ReturnsCheckAnswersView(
        SubmissionStatus status
    )
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        var pageContent = new PageEntry { Sys = new SystemDetails("page-content-id") };
        var response = new QuestionWithAnswerModel
        {
            QuestionSysId = "question-id",
            QuestionSlug = "question-slug",
            QuestionText = "Question text",
            AnswerSysId = "answer-id",
            AnswerText = "Answer text",
        };
        ArrangeFocusedEstablishment();
        ArrangeCheckAnswersContent(section, new PageEntry { Content = [pageContent] });
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.InProgress,
            status,
            new SubmissionResponsesModel(SubmissionId, [response])
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCheckAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug,
            ErrorMessage
        );

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(ReviewAnswersViewBuilder.CheckAnswersViewName, viewResult.ViewName);
        var viewModel = Assert.IsType<ReviewAnswersViewModel>(viewResult.Model);
        Assert.Equal(PageTitleConstants.CheckAnswers, viewModel.Title.Text);
        Assert.Same(pageContent, Assert.Single(viewModel.Content!));
        Assert.Equal(SectionName, viewModel.SectionName);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.Equal(CategoryName, viewModel.CategoryName);
        Assert.Equal(SectionSlug, viewModel.SectionSlug);
        Assert.Equal(UrlConstants.CheckAnswersSlug, viewModel.Slug);
        Assert.Equal(SubmissionId, viewModel.SubmissionId);
        Assert.Equal(ErrorMessage, viewModel.ErrorMessage);
        var submissionResponse = Assert.Single(viewModel.SubmissionResponses!.Responses!);
        Assert.Equal("question-id", submissionResponse.QuestionRef);
        Assert.Equal("answer-id", submissionResponse.AnswerRef);
        Assert.Empty(viewModel.SelectedSchoolNames);
        Assert.False(viewModel.IsMultiSchoolAssessment);
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenUserCanSelectSchools_ReturnsSelectedSchoolNames()
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
                    .WithEstablishmentId(FirstSelectedEstablishmentId)
                    .Build(),
            ]);
        _currentUserProvider.GetSelectedSchoolNames().Returns(["School One", "School Two"]);
        ArrangeCheckAnswersContent(section);
        ArrangeRouting(
            FirstSelectedEstablishmentId,
            section,
            SubmissionStatus.InProgress,
            SubmissionStatus.InProgress,
            new SubmissionResponsesModel(SubmissionId, [])
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCheckAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug
        );

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ReviewAnswersViewModel>(viewResult.Model);
        Assert.Equal(["School One", "School Two"], viewModel.SelectedSchoolNames);
        Assert.Equal(2, viewModel.SelectedSchoolCount);
        Assert.True(viewModel.IsMultiSchoolAssessment);
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenUserCannotSelectSchools_DoesNotReadSelectedSchoolNames()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        ArrangeFocusedEstablishment();
        ArrangeCheckAnswersContent(section);
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.InProgress,
            SubmissionStatus.InProgress,
            new SubmissionResponsesModel(SubmissionId, [])
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToCheckAnswers(new TestController(), CategorySlug, SectionSlug);

        // Assert
        _currentUserProvider.DidNotReceive().GetSelectedSchoolNames();
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenRoutingHasNoSubmission_ReturnsViewModelWithoutSubmission()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        ArrangeFocusedEstablishment();
        ArrangeCheckAnswersContent(section);
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.InProgress,
            SubmissionStatus.InProgress
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCheckAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug
        );

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ReviewAnswersViewModel>(viewResult.Model);
        Assert.Null(viewModel.SubmissionId);
        Assert.Null(viewModel.SubmissionResponses);
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenPageHasNoContent_ReturnsViewModelWithEmptyContent()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        ArrangeFocusedEstablishment();
        ArrangeCheckAnswersContent(section, new PageEntry { Content = null });
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.InProgress,
            SubmissionStatus.InProgress
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCheckAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug
        );

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ReviewAnswersViewModel>(viewResult.Model);
        Assert.Empty(viewModel.Content!);
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenMultipleCategoriesExist_UsesCategoryContainingSection()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        var otherSection = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("other-section-id"),
            Name = "Other Section",
        };
        ArrangeFocusedEstablishment();
        ArrangeCheckAnswersContent(section);
        _contentfulService
            .GetAllCategoriesAsync()
            .Returns([
                new QuestionnaireCategoryEntry
                {
                    Header = new ComponentHeaderEntry { Text = "Other Category" },
                    Sections = [otherSection],
                },
                new QuestionnaireCategoryEntry
                {
                    Header = new ComponentHeaderEntry { Text = CategoryName },
                    Sections = [section],
                },
            ]);
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.InProgress,
            SubmissionStatus.InProgress
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCheckAnswers(
            new TestController(),
            "unrelated-category-slug",
            SectionSlug
        );

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ReviewAnswersViewModel>(viewResult.Model);
        Assert.Equal(CategoryName, viewModel.CategoryName);
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenNoCategoryContainsSection_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
        };
        var otherSection = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("other-section-id"),
            Name = "Other Section",
        };
        ArrangeFocusedEstablishment();
        ArrangeCheckAnswersContent(section);
        _contentfulService
            .GetAllCategoriesAsync()
            .Returns([
                new QuestionnaireCategoryEntry
                {
                    Header = new ComponentHeaderEntry { Text = "Other Category" },
                    Sections = [otherSection],
                },
            ]);
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.InProgress,
            SubmissionStatus.InProgress
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToCheckAnswers(new TestController(), CategorySlug, SectionSlug)
        );
        Assert.Equal($"Could not find category for section {SectionSlug}", exception.Message);
    }

    [Fact]
    public async Task RouteToCheckAnswers_WhenRoutingSectionHasNoId_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry { Name = SectionName };
        ArrangeFocusedEstablishment();
        ArrangeCheckAnswersContent(section);
        ArrangeRouting(
            FocusedEstablishmentId,
            section,
            SubmissionStatus.InProgress,
            SubmissionStatus.InProgress
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToCheckAnswers(new TestController(), CategorySlug, SectionSlug)
        );
        Assert.Equal($"Could not find section id for slug {SectionSlug}", exception.Message);
    }

    [Theory]
    [InlineData(SubmissionStatus.None)]
    [InlineData(SubmissionStatus.Inaccessible)]
    [InlineData(SubmissionStatus.Obsolete)]
    [InlineData(SubmissionStatus.RetrievalError)]
    public async Task RouteToCheckAnswers_WhenStatusIsNotViewable_RedirectsToNextQuestion(
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
            SubmissionStatus.InProgress,
            status,
            nextQuestion: new QuestionnaireQuestionEntry { Slug = NextQuestionSlug }
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToCheckAnswers(
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

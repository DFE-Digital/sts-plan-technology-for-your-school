using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests.Builders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests;

public class RouteBySectionAndRecommendationTests : RecommendationsViewBuilderTestBase
{
    private static readonly DateTime CompletionDate = new(2025, 11, 14, 9, 0, 0, DateTimeKind.Utc);

    private static readonly Dictionary<
        string,
        SqlEstablishmentRecommendationHistoryDto
    > LatestStatuses = new()
    {
        ["chunk-1"] = new SqlEstablishmentRecommendationHistoryDtoBuilder()
            .WithNewStatus(RecommendationStatus.Complete)
            .WithDateCreated(new DateTime(2025, 11, 1))
            .Build(),
        ["chunk-2"] = new SqlEstablishmentRecommendationHistoryDtoBuilder()
            .WithNewStatus(RecommendationStatus.InProgress)
            .WithDateCreated(new DateTime(2025, 11, 2))
            .Build(),
        ["chunk-3"] = new SqlEstablishmentRecommendationHistoryDtoBuilder()
            .WithNewStatus(null)
            .WithDateCreated(new DateTime(2025, 11, 3))
            .Build(),
    };

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenSectionIsNotStarted_RedirectsToHomePage()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), section);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(null, section, null, SubmissionStatus.NotStarted)
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySectionAndRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            useChecklist: false,
            singleChunkSlug: null,
            originatingSlug: null
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetByRoute", redirect.ActionName);
        Assert.Equal("Pages", redirect.ControllerName);
        Assert.Equal("/home", redirect.RouteValues!["route"]);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenSectionIsInProgress_RedirectsToNextQuestion()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        var nextQuestion = new QuestionnaireQuestionEntry { Slug = "next-question" };
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), section);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    nextQuestion,
                    section,
                    null,
                    SubmissionStatus.InProgress
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySectionAndRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            useChecklist: false,
            singleChunkSlug: null,
            originatingSlug: null
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetQuestionBySlug", redirect.ActionName);
        Assert.Equal("Questions", redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues!["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
        Assert.Equal("next-question", redirect.RouteValues["Slug"]);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenSectionIsCompleteNotReviewed_RedirectsToCheckAnswers()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), section);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    section,
                    null,
                    SubmissionStatus.CompleteNotReviewed
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySectionAndRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            useChecklist: false,
            singleChunkSlug: null,
            originatingSlug: null
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("CheckAnswers", redirect.ActionName);
        Assert.Equal("ReviewAnswers", redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues!["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenSubmissionStatusIsNotRoutable_ThrowsInvalidOperationException()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), section);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(null, section, null, SubmissionStatus.Inaccessible)
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.RouteBySectionAndRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                useChecklist: false,
                singleChunkSlug: null,
                originatingSlug: null
            )
        );
        Assert.Equal("Invalid journey status - Inaccessible", exception.Message);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenSectionIsCompleteReviewed_ReturnsRecommendationsViewWithAllChunks()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().WithName("Network security").Build();
        var submission = new SubmissionResponsesModel(1, [new QuestionWithAnswerModel()])
        {
            DateCompleted = CompletionDate,
        };
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().WithHeaderText("Networking").Build(),
            section
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    section,
                    submission,
                    SubmissionStatus.CompleteReviewed
                )
            );
        _recommendationService
            .GetLatestRecommendationStatusesAsync(FocusedEstablishmentId)
            .Returns(LatestStatuses);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySectionAndRecommendation(
            controller,
            CategorySlug,
            SectionSlug,
            useChecklist: false,
            singleChunkSlug: null,
            originatingSlug: SecondChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Recommendations", view.ViewName);
        var viewModel = Assert.IsType<RecommendationsViewModel>(view.Model);
        Assert.Equal("Networking", viewModel.CategoryName);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.Equal("Network security", viewModel.SectionName);
        Assert.Equal(SectionSlug, viewModel.SectionSlug);
        Assert.Equal(SecondChunkSlug, viewModel.OriginatingSlug);
        Assert.Equal("14 Nov 2025", viewModel.LatestCompletionDate);
        Assert.Null(viewModel.CurrentChunkCount);
        Assert.Same(submission.Responses, viewModel.SubmissionResponses);
        Assert.Equal(
            new[] { FirstChunkSlug, SecondChunkSlug, ThirdChunkSlug },
            viewModel.Chunks.Select(chunk => chunk.Slug).ToArray()
        );
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenChunksHaveHistory_ReturnsChunkStatusAndLastUpdated()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), section);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    section,
                    new SubmissionResponsesModel(1, []),
                    SubmissionStatus.CompleteReviewed
                )
            );
        _recommendationService
            .GetLatestRecommendationStatusesAsync(FocusedEstablishmentId)
            .Returns(LatestStatuses);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySectionAndRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            useChecklist: false,
            singleChunkSlug: null,
            originatingSlug: null
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<RecommendationsViewModel>(view.Model);
        Assert.Collection(
            viewModel.Chunks,
            chunk =>
            {
                Assert.Equal(RecommendationStatus.Complete, chunk.Status);
                Assert.Equal(new DateTime(2025, 11, 1), chunk.LastUpdated);
            },
            chunk =>
            {
                Assert.Equal(RecommendationStatus.InProgress, chunk.Status);
                Assert.Equal(new DateTime(2025, 11, 2), chunk.LastUpdated);
            },
            chunk =>
            {
                Assert.Equal(RecommendationStatus.NotStarted, chunk.Status);
                Assert.Equal(new DateTime(2025, 11, 3), chunk.LastUpdated);
            }
        );
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenSubmissionHasNoCompletionDate_ReturnsViewWithoutCompletionDate()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), section);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    section,
                    new SubmissionResponsesModel(1, []),
                    SubmissionStatus.CompleteReviewed
                )
            );
        _recommendationService
            .GetLatestRecommendationStatusesAsync(FocusedEstablishmentId)
            .Returns(LatestStatuses);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySectionAndRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            useChecklist: false,
            singleChunkSlug: null,
            originatingSlug: null
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<RecommendationsViewModel>(view.Model);
        Assert.Null(viewModel.LatestCompletionDate);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenChecklistIsRequested_ReturnsChecklistView()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), section);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    section,
                    new SubmissionResponsesModel(1, []),
                    SubmissionStatus.CompleteReviewed
                )
            );
        _recommendationService
            .GetLatestRecommendationStatusesAsync(FocusedEstablishmentId)
            .Returns(LatestStatuses);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySectionAndRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            useChecklist: true,
            singleChunkSlug: null,
            originatingSlug: null
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("RecommendationsChecklist", view.ViewName);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenSingleChunkSlugIsProvided_ReturnsViewWithOnlyThatChunk()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        var submission = new SubmissionResponsesModel(1, [new QuestionWithAnswerModel()])
        {
            DateCompleted = CompletionDate,
        };
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().WithHeaderText("Networking").Build(),
            section
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    section,
                    submission,
                    SubmissionStatus.CompleteReviewed
                )
            );
        _recommendationService
            .GetLatestRecommendationStatusesAsync(FocusedEstablishmentId)
            .Returns(LatestStatuses);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteBySectionAndRecommendation(
            new TestController(),
            CategorySlug,
            SectionSlug,
            useChecklist: true,
            singleChunkSlug: SecondChunkSlug,
            originatingSlug: ThirdChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("RecommendationsChecklist", view.ViewName);
        var viewModel = Assert.IsType<RecommendationsViewModel>(view.Model);
        var chunk = Assert.Single(viewModel.Chunks);
        Assert.Equal(SecondChunkSlug, chunk.Slug);
        Assert.Equal(2, viewModel.CurrentChunkCount);
        Assert.Equal(3, viewModel.TotalChunks);
        Assert.Equal(SecondChunkSlug, viewModel.OriginatingSlug);
        Assert.Equal("Networking", viewModel.CategoryName);
        Assert.Equal("14 Nov 2025", viewModel.LatestCompletionDate);
        Assert.Same(submission.Responses, viewModel.SubmissionResponses);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenSingleChunkSlugIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), section);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    section,
                    new SubmissionResponsesModel(1, []),
                    SubmissionStatus.CompleteReviewed
                )
            );
        _recommendationService
            .GetLatestRecommendationStatusesAsync(FocusedEstablishmentId)
            .Returns(LatestStatuses);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteBySectionAndRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                useChecklist: false,
                singleChunkSlug: "missing-chunk",
                originatingSlug: null
            )
        );
        Assert.Equal(
            "No recommendation chunk found with slug matching: missing-chunk",
            exception.Message
        );
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenCategoryHasNoLandingPage_ThrowsInvalidDataException()
    {
        // Arrange
        var section = new QuestionnaireSectionEntryBuilder().Build();
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().WithoutLandingPage().Build(),
            section
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _submissionService
            .GetSubmissionRoutingDataAsync(
                FocusedEstablishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    section,
                    new SubmissionResponsesModel(1, []),
                    SubmissionStatus.CompleteReviewed
                )
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.RouteBySectionAndRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                useChecklist: false,
                singleChunkSlug: null,
                originatingSlug: null
            )
        );
        Assert.Equal(
            "Cannot build a recommendations model with an empty category slug",
            exception.Message
        );
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenCategoryIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(null, new QuestionnaireSectionEntryBuilder().Build());
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteBySectionAndRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                useChecklist: false,
                singleChunkSlug: null,
                originatingSlug: null
            )
        );
        Assert.Equal($"Could not find category for slug {CategorySlug}", exception.Message);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), null);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteBySectionAndRecommendation(
                new TestController(),
                CategorySlug,
                SectionSlug,
                useChecklist: false,
                singleChunkSlug: null,
                originatingSlug: null
            )
        );
        Assert.Equal($"Could not find section for slug {SectionSlug}", exception.Message);
    }
}

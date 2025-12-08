using Contentful.Core.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class RecommendationsViewBuilderTests
{
    // ---- Substitutes (collaborators)
    private readonly ILogger<BaseViewBuilder> _logger = Substitute.For<ILogger<BaseViewBuilder>>();
    private readonly IContentfulService _contentful = Substitute.For<IContentfulService>();
    private readonly ISubmissionService _submissions = Substitute.For<ISubmissionService>();
    private readonly IRecommendationService _recommendationService = Substitute.For<IRecommendationService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    // ---- Options
    private ContentfulOptions _contentfulOptions = new ContentfulOptions { UsePreviewApi = false };

    private RecommendationsViewBuilder CreateServiceUnderTest() =>
        new RecommendationsViewBuilder(
            _logger,
            Options.Create(_contentfulOptions),
            _contentful,
            _submissions,
            _recommendationService,
            _currentUser);

    private static Controller MakeController()
    {
        var controller = new DummyController();
        var http = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        controller.TempData = new TempDataDictionary(http, Substitute.For<ITempDataProvider>());
        return controller;
    }

    // ---------- Small builders for domain objects used in tests ----------

    private static QuestionnaireCategoryEntry MakeCategory(string headerText) =>
        new QuestionnaireCategoryEntry { Header = new ComponentHeaderEntry { Text = headerText } };

    private static QuestionnaireSectionEntry MakeSection(string id, string slug, string name = "Section") =>
        new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(id),
            Name = name,
            CoreRecommendations = new List<RecommendationChunkEntry>
            {
                new()
                {
                    Sys = new SystemDetails("C1"),
                    Header = "First Chunk",
                    CompletingAnswers = new List<QuestionnaireAnswerEntry>
                    {
                        new() { Sys = new SystemDetails("C1") }
                    },
                    Slug = "first-chunk-1"
                },
                new()
                {
                    Sys = new SystemDetails("C2"),
                    Header = "Second Chunk",
                    CompletingAnswers = new List<QuestionnaireAnswerEntry>
                    {
                        new() { Sys = new SystemDetails("C2") }
                    },
                    Slug = "second-chunk-2"
                },
                new()
                {
                    Sys = new SystemDetails("C3"),
                    Header = "Third Chunk",
                    CompletingAnswers = new List<QuestionnaireAnswerEntry>
                    {
                        new() { Sys = new SystemDetails("C3") }
                    },
                    Slug = "third-chunk-3"
                }
            }
        };

    private static SubmissionRoutingDataModel MakeRouting(
        SubmissionStatus status,
        QuestionnaireSectionEntry section,
        string? nextQuestionSlug = null,
        DateTime? completed = null,
        params string[] answerSysIds)
    {
        var nextQuestion = nextQuestionSlug == null ? null : new QuestionnaireQuestionEntry { Slug = nextQuestionSlug };
        var submission = new SubmissionResponsesModel(1, answerSysIds.Select(id => new QuestionWithAnswerModel { AnswerSysId = id }).ToList())
        {
            DateCompleted = completed
        };

        return new SubmissionRoutingDataModel(nextQuestion, section, submission, status);
    }

    // ---------- RouteToSingleRecommendation ----------

    [Fact]
    public async Task RouteToSingleRecommendation_Renders_SingleRecommendation_With_PrevNext()
    {
        // Arrange - Setup recommendation service to return status data for integration testing
        var sut = CreateServiceUnderTest();
        var ctl = MakeController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);

        var categorySlug = "cat-a";
        var section = MakeSection("S1", "sec-1", "Section One");
        _contentful.GetCategoryHeaderTextBySlugAsync(categorySlug).Returns("Networking");
        _contentful.GetSectionBySlugAsync("sec-1", 2).Returns(section);

        // Submission has answers that match chunk ids "C1","C2","C3"
        var routing = MakeRouting(SubmissionStatus.CompleteReviewed, section, answerSysIds: new[] { "C1", "C2", "C3" });
        _submissions.GetSubmissionRoutingDataAsync(123, section, true).Returns(routing);

        // Setup recommendation service with status data for the specific chunk being tested
        var currentRecommendationStatus = new SqlEstablishmentRecommendationHistoryDto
        {
            EstablishmentId = 123,
            RecommendationId = 2,
            UserId = 1,
            NewStatus = "Completed",
            DateCreated = DateTime.UtcNow.AddDays(-1)
        };

        _recommendationService.GetCurrentRecommendationStatusAsync("C2", 123)
            .Returns(currentRecommendationStatus);

        // Act (choose middle chunk to test prev/next both populated)
        var result = await sut.RouteToSingleRecommendation(ctl, categorySlug, "sec-1", "second-chunk-2", useChecklist: false);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("SingleRecommendation", view.ViewName);

        var vm = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Equal("Networking", vm.CategoryName);
        Assert.Equal("cat-a", vm.CategorySlug);
        Assert.Equal("sec-1", vm.SectionSlug);
        Assert.Equal(3, vm.Chunks.Count);
        Assert.Equal("second-chunk-2", vm.CurrentChunk.Slug);
        Assert.Equal(2, vm.CurrentChunkPosition);
        Assert.Equal(3, vm.TotalChunks);
        Assert.NotNull(vm.PreviousChunk);
        Assert.NotNull(vm.NextChunk);
        Assert.Equal("first-chunk-1", vm.PreviousChunk!.Slug);
        Assert.Equal("third-chunk-3", vm.NextChunk!.Slug);

        Assert.Equal("Completed", vm.SelectedStatusKey);
        Assert.Equal(DateTime.UtcNow.AddDays(-1).Date, vm.LastUpdated?.Date);

        await _recommendationService.Received(1).GetCurrentRecommendationStatusAsync("C2", 123);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_Throws_When_Chunk_Not_Found()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = MakeController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);
        var section = MakeSection("S1", "sec-1");

        _contentful.GetCategoryHeaderTextBySlugAsync("cat").Returns("Header");
        _contentful.GetSectionBySlugAsync("sec-1", 2).Returns(section);

        var routing = MakeRouting(SubmissionStatus.CompleteReviewed, section, answerSysIds: new[] { "C1" });
        _submissions.GetSubmissionRoutingDataAsync(123, section, true).Returns(routing);

        // Act + Assert
        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteToSingleRecommendation(ctl, "cat", "sec-1", "missing-slug", false));
    }

    // ---------- RouteBySectionAndRecommendation ----------

    [Fact]
    public async Task RouteBySectionAndRecommendation_NotStarted_Redirects_Home()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = MakeController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);
        var category = MakeCategory("Cat");
        var section = MakeSection("S1", "sec-1");

        _contentful.GetCategoryBySlugAsync("cat").Returns(category);
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        var routing = MakeRouting(SubmissionStatus.NotStarted, section);
        _submissions.GetSubmissionRoutingDataAsync(1, section, true).Returns(routing);

        // Act
        var result = await sut.RouteBySectionAndRecommendation(ctl, "cat", "sec-1", useChecklist: false);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_InProgress_Redirects_To_Next_Question()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = MakeController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);
        var category = MakeCategory("Cat");
        var section = MakeSection("S1", "sec-1");

        _contentful.GetCategoryBySlugAsync("cat").Returns(category);
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        var routing = MakeRouting(SubmissionStatus.InProgress, section, nextQuestionSlug: "q-2");
        _submissions.GetSubmissionRoutingDataAsync(1, section, true).Returns(routing);

        // Act
        var result = await sut.RouteBySectionAndRecommendation(ctl, "cat", "sec-1", useChecklist: false);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.GetQuestionBySlug), redirect.ActionName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal("cat", redirect.RouteValues["categorySlug"]);
        Assert.Equal("sec-1", redirect.RouteValues["sectionSlug"]);
        Assert.Equal("q-2", redirect.RouteValues["Slug"] ?? redirect.RouteValues["questionSlug"]); // handle either anon type naming
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_CompleteNotReviewed_Redirects_To_CheckAnswers()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = MakeController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);
        var category = MakeCategory("Cat");
        var section = MakeSection("S1", "sec-1");

        _contentful.GetCategoryBySlugAsync("cat").Returns(category);
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        var routing = MakeRouting(SubmissionStatus.CompleteNotReviewed, section);
        _submissions.GetSubmissionRoutingDataAsync(1, section, true).Returns(routing);

        // Act
        var result = await sut.RouteBySectionAndRecommendation(ctl, "cat", "sec-1", useChecklist: false);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_CompleteReviewed_Renders_Recommendations_View()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = MakeController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);
        var category = MakeCategory("Connectivity");
        var section = MakeSection("S1", "sec-1");

        _contentful.GetCategoryBySlugAsync("connectivity").Returns(category);
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        var routing = MakeRouting(
            SubmissionStatus.CompleteReviewed,
            section,
            "nextQuestionSlug",
            completed: new DateTime(2024, 1, 2),
            "C1", "C2");
        _submissions.GetSubmissionRoutingDataAsync(1, section, true).Returns(routing);

        _recommendationService
            .GetLatestRecommendationStatusesAsync(Arg.Any<int>())
            .Returns(new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
            {
                ["C1"] = new SqlEstablishmentRecommendationHistoryDto
                {
                    RecommendationId = 1,
                    DateCreated = DateTime.UtcNow,
                    NewStatus = "InProgress"
                },
                ["C2"] = new SqlEstablishmentRecommendationHistoryDto
                {
                    RecommendationId = 2,
                    DateCreated = DateTime.UtcNow,
                    NewStatus = "Complete"
                },
                ["C3"] = new SqlEstablishmentRecommendationHistoryDto
                {
                    RecommendationId = 3,
                    DateCreated = DateTime.UtcNow,
                    NewStatus = "NotStarted"
                }
            });

        // Act (useChecklist=false -> "Recommendations"; true -> "RecommendationsChecklist")
        var result = await sut.RouteBySectionAndRecommendation(ctl, "connectivity", "sec-1", useChecklist: false);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Recommendations", view.ViewName);
        var vm = Assert.IsType<RecommendationsViewModel>(view.Model);
        Assert.Equal("Connectivity", vm.CategoryName);
        Assert.Equal("Section", vm.SectionName);
        Assert.Equal("sec-1", vm.SectionSlug);
        Assert.Equal(3, vm.Chunks.Count);
        Assert.NotNull(routing.Submission);
        Assert.Equal(routing.Submission.Responses, vm.SubmissionResponses);
        Assert.NotNull(vm.LatestCompletionDate);
    }


    [Fact]
    public async Task RouteBySectionAndRecommendation_CompleteReviewed_Renders_Checklist_When_Requested()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = MakeController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);
        var category = MakeCategory("Connectivity");
        var section = MakeSection("S1", "sec-1");

        _contentful.GetCategoryBySlugAsync("connectivity").Returns(category);
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        var routing = MakeRouting(SubmissionStatus.CompleteReviewed, section, answerSysIds: "C1");
        _submissions.GetSubmissionRoutingDataAsync(1, section, true).Returns(routing);

        _recommendationService
            .GetLatestRecommendationStatusesAsync(Arg.Any<int>())
            .Returns(new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
            {
                ["C1"] = new SqlEstablishmentRecommendationHistoryDto
                {
                    RecommendationId = 1,
                    DateCreated = DateTime.UtcNow,
                    NewStatus = "InProgress"
                },
                ["C2"] = new SqlEstablishmentRecommendationHistoryDto
                {
                    RecommendationId = 2,
                    DateCreated = DateTime.UtcNow,
                    NewStatus = "InProgress"
                },
                ["C3"] = new SqlEstablishmentRecommendationHistoryDto
                {
                    RecommendationId = 3,
                    DateCreated = DateTime.UtcNow,
                    NewStatus = "InProgress"
                }
            });

        // Act
        var result = await sut.RouteBySectionAndRecommendation(ctl, "connectivity", "sec-1", useChecklist: true);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("RecommendationsChecklist", view.ViewName);
        var vm = Assert.IsType<RecommendationsViewModel>(view.Model);
        Assert.Equal(3, vm.Chunks.Count);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_WithSingleChunkSlug_Renders_Single_Recommendation()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = MakeController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);

        var categorySlug = "connectivity";
        var sectionSlug = "sec-1";
        var category = MakeCategory("Connectivity");

        var section = MakeSection("S1", sectionSlug);

        _contentful.GetCategoryBySlugAsync(categorySlug).Returns(category);
        _contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        var routing = MakeRouting(
            SubmissionStatus.CompleteReviewed,
            section,
            answerSysIds: new[] { "C1", "C2", "C3" });

        _submissions.GetSubmissionRoutingDataAsync(1, section, true).Returns(routing);

        _recommendationService
            .GetLatestRecommendationStatusesAsync(Arg.Any<int>())
            .Returns(new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
            {
                ["C1"] = new() { RecommendationId = 1, DateCreated = DateTime.UtcNow.AddDays(-1), NewStatus = "Completed" },
                ["C2"] = new() { RecommendationId = 2, DateCreated = DateTime.UtcNow.AddDays(-2), NewStatus = "Completed" },
                ["C3"] = new() { RecommendationId = 3, DateCreated = DateTime.UtcNow.AddDays(-3), NewStatus = "Completed" }
            });

        const string expectedSlug = "second-chunk-2";

        // Act
        var result = await sut.RouteBySectionAndRecommendation(
            ctl,
            categorySlug,
            sectionSlug,
            useChecklist: false,
            singleChunkSlug: expectedSlug,
            originatingSlug: expectedSlug);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Recommendations", view.ViewName);

        var vm = Assert.IsType<RecommendationsViewModel>(view.Model);

        Assert.Single(vm.Chunks);
        Assert.Equal("Second Chunk", vm.Chunks[0].Header);
        Assert.Equal(2, vm.CurrentChunkCount);
        Assert.Equal(3, vm.TotalChunks);

        Assert.Equal("Connectivity", vm.CategoryName);
        Assert.Equal(sectionSlug, vm.SectionSlug);
        Assert.Equal(routing.Submission?.Responses, vm.SubmissionResponses);
        Assert.Equal(expectedSlug, vm.OriginatingSlug);
    }

    // ---------- Support ----------

    private sealed class DummyController : Controller { }
}

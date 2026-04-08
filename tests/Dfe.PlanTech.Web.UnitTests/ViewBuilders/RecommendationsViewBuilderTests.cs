using Contentful.Core.Configuration;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class RecommendationsViewBuilderTests
{
    private const string DefaultRecipient = "test@test.com";

    // ---- Substitutes (collaborators)
    private readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly INotifyService _notifyService = Substitute.For<INotifyService>();
    private readonly IRecommendationService _recommendationService =
        Substitute.For<IRecommendationService>();
    private readonly IMicrocopyProvider _microcopyProvider = Substitute.For<IMicrocopyProvider>();
    private readonly ISubmissionService _submissions = Substitute.For<ISubmissionService>();

    // ---- Options
    private ContentfulOptions _contentfulOptions = new ContentfulOptions { UsePreviewApi = false };
    private static readonly string[] c1 = ["C1"];
    private static readonly string[] c123 = ["C1", "C2", "C3"];

    private RecommendationsViewBuilder CreateServiceUnderTest() =>
        new RecommendationsViewBuilder(
            NullLogger<BaseViewBuilder>.Instance,
            _contentfulService,
            _currentUser,
            _notifyService,
            _recommendationService,
            _submissions,
            _microcopyProvider
        );

    private static TestController CreateController() => new TestController();

    // ---------- Small builders for domain objects used in tests ----------

    private static QuestionnaireCategoryEntry CreateCategory(
        string headerText,
        string? landingPageSlug = null
    )
    {
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = headerText },
        };

        if (!string.IsNullOrWhiteSpace(landingPageSlug))
        {
            category.LandingPage = new PageEntry { Slug = landingPageSlug };
        }

        return category;
    }

    private static QuestionnaireSectionEntry CreateSection(
        string id,
        string slug,
        string name = "Section"
    ) =>
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
                        new() { Sys = new SystemDetails("C1") },
                    },
                    Slug = "first-chunk-1",
                },
                new()
                {
                    Sys = new SystemDetails("C2"),
                    Header = "Second Chunk",
                    CompletingAnswers = new List<QuestionnaireAnswerEntry>
                    {
                        new() { Sys = new SystemDetails("C2") },
                    },
                    Slug = "second-chunk-2",
                },
                new()
                {
                    Sys = new SystemDetails("C3"),
                    Header = "Third Chunk",
                    CompletingAnswers = new List<QuestionnaireAnswerEntry>
                    {
                        new() { Sys = new SystemDetails("C3") },
                    },
                    Slug = "third-chunk-3",
                },
            },
        };

    private static SubmissionRoutingDataModel CreateRouting(
        SubmissionStatus status,
        QuestionnaireSectionEntry section,
        string? nextQuestionSlug = null,
        DateTime? completed = null,
        params string[] answerSysIds
    )
    {
        var nextQuestion =
            nextQuestionSlug == null
                ? null
                : new QuestionnaireQuestionEntry { Slug = nextQuestionSlug };
        var submission = new SubmissionResponsesModel(
            1,
            answerSysIds.Select(id => new QuestionWithAnswerModel { AnswerSysId = id }).ToList()
        )
        {
            DateCompleted = completed,
        };

        return new SubmissionRoutingDataModel(nextQuestion, section, submission, status);
    }

    // ---------- RouteToSingleRecommendation ----------

    [Fact]
    public async Task RouteToSingleRecommendation_Renders_SingleRecommendation_With_PrevNext()
    {
        // Arrange - Setup recommendation service to return status data for integration testing
        var sut = CreateServiceUnderTest();
        var ctl = CreateController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);

        var categorySlug = "cat-a";
        var section = CreateSection("S1", "sec-1", "Section One");
        _contentfulService.GetCategoryHeaderTextBySlugAsync(categorySlug).Returns("Networking");
        _contentfulService.GetSectionBySlugAsync("sec-1", 2).Returns(section);

        // Submission has answers that match chunk ids "C1","C2","C3"
        var routing = CreateRouting(SubmissionStatus.CompleteReviewed, section, answerSysIds: c123);
        _submissions
            .GetSubmissionRoutingDataAsync(123, section, SubmissionStatus.CompleteReviewed)
            .Returns(routing);

        // Setup recommendation service with status data for the specific chunk being tested
        var currentRecommendationStatus = new SqlEstablishmentRecommendationHistoryDto
        {
            EstablishmentId = 123,
            RecommendationId = 2,
            UserId = 1,
            NewStatus = RecommendationStatus.Complete,
            DateCreated = DateTime.UtcNow.AddDays(-1),
        };

        _recommendationService
            .GetLatestRecommendationHistoryAsync("C2", 123)
            .Returns(currentRecommendationStatus);

        // Setup recommendation service with history

        var recommendationHistory1 = new SqlEstablishmentRecommendationHistoryDto
        {
            DateCreated = new DateTime(2025, 11, 14),
        };

        var recommendationHistory2 = new SqlEstablishmentRecommendationHistoryDto
        {
            DateCreated = new DateTime(2025, 11, 11),
        };

        _recommendationService
            .GetRecommendationHistoryAsync("C2", 123)
            .Returns([recommendationHistory1, recommendationHistory2]);

        var expectedDictionary = new Dictionary<
            string,
            IEnumerable<SqlEstablishmentRecommendationHistoryDto>
        >
        {
            { "November activity", [recommendationHistory2, recommendationHistory1] },
        };

        // Act (choose middle chunk to test prev/next both populated)
        var result = await sut.RouteToSingleRecommendation(
            ctl,
            categorySlug,
            "sec-1",
            "second-chunk-2",
            useChecklist: false
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("SingleRecommendation", view.ViewName);

        var vm = Assert.IsType<SingleRecommendationViewModel>(view.Model);
        Assert.Equal("Networking", vm.CategoryName);
        Assert.Equal("cat-a", vm.CategorySlug);
        Assert.Equal("sec-1", vm.SectionSlug);
        Assert.Equal(3, vm.Chunks.Count);
        Assert.Equal("second-chunk-2", vm.CurrentChunk.Slug);
        Assert.NotNull(vm.PreviousChunk);
        Assert.NotNull(vm.NextChunk);
        Assert.Equal(2, vm.CurrentChunkPosition);
        Assert.Equal(3, vm.TotalChunks);
        Assert.Equal("first-chunk-1", vm.PreviousChunk!.Slug);
        Assert.Equal("third-chunk-3", vm.NextChunk!.Slug);
        Assert.Equal(RecommendationStatus.Complete, vm.SelectedStatusKey);
        Assert.Equal(DateTime.UtcNow.AddDays(-1).Date, vm.LastUpdated?.Date);
        Assert.Equal("second-chunk-2", vm.OriginatingSlug);
        Assert.Single(vm.History.Keys);
        Assert.Equal(
            new DateTime(2025, 11, 14),
            vm.History.Values.First().Take(1).First().DateCreated
        );
        Assert.Equal(
            new DateTime(2025, 11, 11),
            vm.History.Values.First().Skip(1).Take(1).First().DateCreated
        );
        Assert.Equal("second-chunk-2", vm.OriginatingSlug);

        await _recommendationService.Received(1).GetLatestRecommendationHistoryAsync("C2", 123);
    }

    [Fact]
    public async Task RouteToSingleRecommendation_Throws_When_Chunk_Not_Found()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = CreateController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);
        var section = CreateSection("S1", "sec-1");

        _contentfulService.GetCategoryHeaderTextBySlugAsync("cat").Returns("Header");
        _contentfulService.GetSectionBySlugAsync("sec-1", 2).Returns(section);

        var routing = CreateRouting(SubmissionStatus.CompleteReviewed, section, answerSysIds: c1);
        _submissions
            .GetSubmissionRoutingDataAsync(123, section, SubmissionStatus.CompleteReviewed)
            .Returns(routing);

        // Act + Assert
        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteToSingleRecommendation(ctl, "cat", "sec-1", "missing-slug", false)
        );
    }

    // ---------- RouteBySectionAndRecommendation ----------

    [Fact]
    public async Task RouteBySectionAndRecommendation_NotStarted_Redirects_Home()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = CreateController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);
        var category = CreateCategory("Cat");
        var section = CreateSection("S1", "sec-1");

        _contentfulService.GetCategoryBySlugAsync("cat").Returns(category);
        _contentfulService.GetSectionBySlugAsync("sec-1").Returns(section);

        var routing = CreateRouting(SubmissionStatus.NotStarted, section);
        _submissions
            .GetSubmissionRoutingDataAsync(1, section, SubmissionStatus.CompleteReviewed)
            .Returns(routing);

        // Act
        var result = await sut.RouteBySectionAndRecommendation(
            ctl,
            "cat",
            "sec-1",
            useChecklist: false,
            null,
            null
        );

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_InProgress_Redirects_To_Next_Question()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = CreateController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);
        var category = CreateCategory("Cat");
        var section = CreateSection("S1", "sec-1");

        _contentfulService.GetCategoryBySlugAsync("cat").Returns(category);
        _contentfulService.GetSectionBySlugAsync("sec-1").Returns(section);

        var routing = CreateRouting(SubmissionStatus.InProgress, section, nextQuestionSlug: "q-2");
        _submissions
            .GetSubmissionRoutingDataAsync(1, section, SubmissionStatus.CompleteReviewed)
            .Returns(routing);

        // Act
        var result = await sut.RouteBySectionAndRecommendation(
            ctl,
            "cat",
            "sec-1",
            useChecklist: false,
            null,
            null
        );

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
        var ctl = CreateController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);
        var category = CreateCategory("Cat");
        var section = CreateSection("S1", "sec-1");

        _contentfulService.GetCategoryBySlugAsync("cat").Returns(category);
        _contentfulService.GetSectionBySlugAsync("sec-1").Returns(section);

        var routing = CreateRouting(SubmissionStatus.CompleteNotReviewed, section);
        _submissions
            .GetSubmissionRoutingDataAsync(1, section, SubmissionStatus.CompleteReviewed)
            .Returns(routing);

        // Act
        var result = await sut.RouteBySectionAndRecommendation(
            ctl,
            "cat",
            "sec-1",
            useChecklist: false,
            null,
            null
        );

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_CompleteReviewed_Renders_Recommendations_View()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = CreateController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);
        var category = CreateCategory("Connectivity", "connectivity");
        var section = CreateSection("S1", "sec-1");

        _contentfulService.GetCategoryBySlugAsync("connectivity").Returns(category);
        _contentfulService.GetSectionBySlugAsync("sec-1").Returns(section);

        var routing = CreateRouting(
            SubmissionStatus.CompleteReviewed,
            section,
            "nextQuestionSlug",
            completed: new DateTime(2024, 1, 2),
            "C1",
            "C2"
        );
        _submissions
            .GetSubmissionRoutingDataAsync(1, section, SubmissionStatus.CompleteReviewed)
            .Returns(routing);

        _recommendationService
            .GetLatestRecommendationStatusesAsync(Arg.Any<int>())
            .Returns(
                new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
                {
                    ["C1"] = new SqlEstablishmentRecommendationHistoryDto
                    {
                        RecommendationId = 1,
                        DateCreated = DateTime.UtcNow,
                        NewStatus = RecommendationStatus.InProgress,
                    },
                    ["C2"] = new SqlEstablishmentRecommendationHistoryDto
                    {
                        RecommendationId = 2,
                        DateCreated = DateTime.UtcNow,
                        NewStatus = RecommendationStatus.Complete,
                    },
                    ["C3"] = new SqlEstablishmentRecommendationHistoryDto
                    {
                        RecommendationId = 3,
                        DateCreated = DateTime.UtcNow,
                        NewStatus = RecommendationStatus.NotStarted,
                    },
                }
            );

        // Act (useChecklist=false -> "Recommendations"; true -> "RecommendationsChecklist")
        var result = await sut.RouteBySectionAndRecommendation(
            ctl,
            "connectivity",
            "sec-1",
            useChecklist: false,
            null,
            null
        );

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
        var categoryTitle = "Connectivity";
        var slug = categoryTitle.ToLower();

        var sut = CreateServiceUnderTest();
        var ctl = CreateController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);
        var category = CreateCategory(categoryTitle, slug);
        var section = CreateSection("S1", "sec-1");

        _contentfulService.GetCategoryBySlugAsync("connectivity").Returns(category);
        _contentfulService.GetSectionBySlugAsync("sec-1").Returns(section);

        var routing = CreateRouting(SubmissionStatus.CompleteReviewed, section, answerSysIds: "C1");
        _submissions
            .GetSubmissionRoutingDataAsync(1, section, SubmissionStatus.CompleteReviewed)
            .Returns(routing);

        _recommendationService
            .GetLatestRecommendationStatusesAsync(Arg.Any<int>())
            .Returns(
                new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
                {
                    ["C1"] = new SqlEstablishmentRecommendationHistoryDto
                    {
                        RecommendationId = 1,
                        DateCreated = DateTime.UtcNow,
                        NewStatus = RecommendationStatus.InProgress,
                    },
                    ["C2"] = new SqlEstablishmentRecommendationHistoryDto
                    {
                        RecommendationId = 2,
                        DateCreated = DateTime.UtcNow,
                        NewStatus = RecommendationStatus.InProgress,
                    },
                    ["C3"] = new SqlEstablishmentRecommendationHistoryDto
                    {
                        RecommendationId = 3,
                        DateCreated = DateTime.UtcNow,
                        NewStatus = RecommendationStatus.InProgress,
                    },
                }
            );

        // Act
        var result = await sut.RouteBySectionAndRecommendation(
            ctl,
            slug,
            "sec-1",
            useChecklist: true,
            null,
            null
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("RecommendationsChecklist", view.ViewName);
        var vm = Assert.IsType<RecommendationsViewModel>(view.Model);
        Assert.Equal(3, vm.Chunks.Count);
    }

    [Fact]
    public async Task RouteBySectionAndRecommendation_Throws_If_No_LangingPage()
    {
        // Arrange
        var categoryTitle = "Connectivity";
        var slug = categoryTitle.ToLower();

        var sut = CreateServiceUnderTest();
        var ctl = CreateController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);
        var category = CreateCategory(categoryTitle, slug);
        var section = CreateSection("S1", "sec-1");

        _contentfulService.GetCategoryBySlugAsync(slug).Returns(category);
        _contentfulService.GetSectionBySlugAsync("sec-1").Returns(section);

        var routing = CreateRouting(SubmissionStatus.CompleteReviewed, section, answerSysIds: "C1");
        _submissions
            .GetSubmissionRoutingDataAsync(1, section, SubmissionStatus.CompleteReviewed)
            .Returns(routing);

        _recommendationService
            .GetLatestRecommendationStatusesAsync(Arg.Any<int>())
            .Returns(
                new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
                {
                    ["C1"] = new SqlEstablishmentRecommendationHistoryDto
                    {
                        RecommendationId = 1,
                        DateCreated = DateTime.UtcNow,
                        NewStatus = RecommendationStatus.InProgress,
                    },
                    ["C2"] = new SqlEstablishmentRecommendationHistoryDto
                    {
                        RecommendationId = 2,
                        DateCreated = DateTime.UtcNow,
                        NewStatus = RecommendationStatus.InProgress,
                    },
                    ["C3"] = new SqlEstablishmentRecommendationHistoryDto
                    {
                        RecommendationId = 3,
                        DateCreated = DateTime.UtcNow,
                        NewStatus = RecommendationStatus.InProgress,
                    },
                }
            );

        // Act
        var result = await sut.RouteBySectionAndRecommendation(
            ctl,
            slug,
            "sec-1",
            useChecklist: true,
            null,
            null
        );

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
        var categoryTitle = "Connectivity";
        var categorySlug = categoryTitle.ToLower();
        var sectionSlug = "sec-1";

        var sut = CreateServiceUnderTest();
        var ctl = CreateController();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(1);

        var category = CreateCategory(categoryTitle, categorySlug);
        var section = CreateSection("S1", sectionSlug);

        _contentfulService.GetCategoryBySlugAsync(categorySlug).Returns(category);
        _contentfulService.GetSectionBySlugAsync(sectionSlug).Returns(section);

        var routing = CreateRouting(SubmissionStatus.CompleteReviewed, section, answerSysIds: c123);

        _submissions
            .GetSubmissionRoutingDataAsync(1, section, SubmissionStatus.CompleteReviewed)
            .Returns(routing);

        _recommendationService
            .GetLatestRecommendationStatusesAsync(Arg.Any<int>())
            .Returns(
                new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
                {
                    ["C1"] = new()
                    {
                        RecommendationId = 1,
                        DateCreated = DateTime.UtcNow.AddDays(-1),
                        NewStatus = RecommendationStatus.Complete,
                    },
                    ["C2"] = new()
                    {
                        RecommendationId = 2,
                        DateCreated = DateTime.UtcNow.AddDays(-2),
                        NewStatus = RecommendationStatus.Complete,
                    },
                    ["C3"] = new()
                    {
                        RecommendationId = 3,
                        DateCreated = DateTime.UtcNow.AddDays(-3),
                        NewStatus = RecommendationStatus.Complete,
                    },
                }
            );

        const string expectedSlug = "second-chunk-2";

        // Act
        var result = await sut.RouteBySectionAndRecommendation(
            ctl,
            categorySlug,
            sectionSlug,
            useChecklist: false,
            singleChunkSlug: expectedSlug,
            originatingSlug: expectedSlug
        );

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

    // ---------- RouteToShareRecommendationAsync ----------

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenInputModelIsNull_ReturnsShareView()
    {
        var controller = CreateController();
        var category = CreateCategory("category");
        var section = CreateSection(
            "section",
            CreateRecommendationChunk("chunk", "chunk-id", "Use MFA", "text-body-id")
        );

        _contentfulService.GetCategoryBySlugAsync("category").Returns(category);
        _contentfulService.GetSectionBySlugAsync("section").Returns(section);

        var sut = CreateServiceUnderTest();

        var result = await sut.RouteToShareRecommendationAsync(
            controller,
            "category",
            "section",
            "chunk",
            null
        );

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Shared/Email/ShareByEmail.cshtml", viewResult.ViewName);

        var model = Assert.IsType<ShareByEmailViewModel>(viewResult.Model);
        Assert.Equal("Recommendations", model.PostController);
        Assert.Equal(nameof(RecommendationsController.ShareSingleRecommendation), model.PostAction);
        Assert.Equal("category", model.CategorySlug);
        Assert.Equal("section", model.SectionSlug);
        Assert.Equal("chunk", model.ChunkSlug);
        Assert.Equal("Use MFA", model.Caption);
        Assert.Equal("Share a recommendation by email", model.Heading);
        Assert.Null(model.InputModel);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenModelStateIsInvalid_ReturnsShareViewWithInputModel()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("EmailAddresses", "Nope");

        var inputModel = new ShareByEmailInputViewModel
        {
            NameOfUser = "Drew",
            EmailAddresses = ["test@example.com"],
        };

        var category = CreateCategory("category");
        var section = CreateSection(
            "section",
            CreateRecommendationChunk("chunk", "chunk-id", "Use MFA", "text-body-id")
        );

        _contentfulService.GetCategoryBySlugAsync("category").Returns(category);
        _contentfulService.GetSectionBySlugAsync("section").Returns(section);

        var sut = CreateServiceUnderTest();

        var result = await sut.RouteToShareRecommendationAsync(
            controller,
            "category",
            "section",
            "chunk",
            inputModel
        );

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Shared/Email/ShareByEmail.cshtml", viewResult.ViewName);

        var model = Assert.IsType<ShareByEmailViewModel>(viewResult.Model);
        Assert.Same(inputModel, model.InputModel);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenCategoryNotFound_ThrowsContentfulDataUnavailableException()
    {
        var controller = CreateController();

        _contentfulService
            .GetCategoryBySlugAsync("missing-category")
            .Returns((QuestionnaireCategoryEntry?)null);

        var sut = CreateServiceUnderTest();

        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteToShareRecommendationAsync(
                controller,
                "missing-category",
                "section",
                "chunk",
                new ShareByEmailInputViewModel()
            )
        );

        Assert.Equal("Could not find category for slug missing-category", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenSectionNotFound_ThrowsContentfulDataUnavailableException()
    {
        var controller = CreateController();

        _contentfulService.GetCategoryBySlugAsync("category").Returns(CreateCategory("category"));
        _contentfulService
            .GetSectionBySlugAsync("missing-section")
            .Returns((QuestionnaireSectionEntry)null!);

        var sut = CreateServiceUnderTest();

        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteToShareRecommendationAsync(
                controller,
                "category",
                "missing-section",
                "chunk",
                new ShareByEmailInputViewModel()
            )
        );

        Assert.Equal("Could not find section for slug missing-section", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenChunkNotFound_ThrowsContentfulDataUnavailableException()
    {
        var controller = CreateController();

        var category = CreateCategory("category");
        var section = CreateSection(
            "section",
            CreateRecommendationChunk("different-chunk", "chunk-id", "Use MFA", "text-body-id")
        );

        _contentfulService.GetCategoryBySlugAsync("category").Returns(category);
        _contentfulService.GetSectionBySlugAsync("section").Returns(section);

        var sut = CreateServiceUnderTest();

        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteToShareRecommendationAsync(
                controller,
                "category",
                "section",
                "missing-chunk",
                new ShareByEmailInputViewModel()
            )
        );

        Assert.Equal("Could not find chunk for slug missing-chunk", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenRecommendationStatusMissing_ThrowsInvalidDataException()
    {
        var controller = CreateController();
        var inputModel = CreateInputModel();

        var chunk = CreateRecommendationChunk("chunk", "chunk-id", "Use MFA", "text-body-id");
        var section = CreateSection("section", chunk);
        var category = CreateCategory("category");

        _contentfulService.GetCategoryBySlugAsync("category").Returns(category);
        _contentfulService.GetSectionBySlugAsync("section").Returns(section);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);

        _recommendationService
            .GetLatestRecommendationHistoryAsync("chunk-id", 123)
            .Returns(new SqlEstablishmentRecommendationHistoryDto { NewStatus = null });

        var sut = CreateServiceUnderTest();

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.RouteToShareRecommendationAsync(
                controller,
                "category",
                "section",
                "chunk",
                inputModel
            )
        );

        Assert.Equal("Cannot send an email without a recommendation status", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenTextBodyMissing_ThrowsContentfulDataUnavailableException()
    {
        var controller = CreateController();
        var inputModel = CreateInputModel();

        var chunk = CreateRecommendationChunk("chunk", "chunk-id", "Use MFA", "text-body-id");
        var section = CreateSection("section", chunk);
        var category = CreateCategory("category");

        _contentfulService.GetCategoryBySlugAsync("category").Returns(category);
        _contentfulService.GetSectionBySlugAsync("section").Returns(section);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);

        _recommendationService
            .GetLatestRecommendationHistoryAsync("chunk-id", 123)
            .Returns(
                new SqlEstablishmentRecommendationHistoryDto
                {
                    NewStatus = RecommendationStatus.InProgress,
                }
            );

        _contentfulService
            .GetTextBodyByIdAsync("text-body-id")
            .Returns((ComponentTextBodyEntry)null!);

        var sut = CreateServiceUnderTest();

        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteToShareRecommendationAsync(
                controller,
                "category",
                "section",
                "chunk",
                inputModel
            )
        );

        Assert.Equal("Could not find text body entry for id text-body-id", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenEstablishmentNameMissing_ThrowsInvalidDataException()
    {
        var controller = CreateController();
        var inputModel = CreateInputModel();

        var textBody = new ComponentTextBodyEntry
        {
            Sys = new SystemDetails { Id = "text-body-id" },
        };
        var chunk = CreateRecommendationChunk("chunk", "chunk-id", "Use MFA", "text-body-id");
        var section = CreateSection("section", chunk);
        var category = CreateCategory("category");

        _contentfulService.GetCategoryBySlugAsync("category").Returns(category);
        _contentfulService.GetSectionBySlugAsync("section").Returns(section);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);
        _currentUser.GetActiveEstablishmentNameAsync().Returns((string?)null);

        _recommendationService
            .GetLatestRecommendationHistoryAsync("chunk-id", 123)
            .Returns(
                new SqlEstablishmentRecommendationHistoryDto
                {
                    NewStatus = RecommendationStatus.InProgress,
                }
            );

        _contentfulService.GetTextBodyByIdAsync("text-body-id").Returns(textBody);

        var sut = CreateServiceUnderTest();

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.RouteToShareRecommendationAsync(
                controller,
                "category",
                "section",
                "chunk",
                inputModel
            )
        );

        Assert.Equal(
            "Cannot send an email without an active establishment name",
            exception.Message
        );
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenNotifySendSucceeds_RedirectsBackToSingleRecommendation()
    {
        var controller = CreateController();
        var inputModel = CreateInputModel();

        var textBody = new ComponentTextBodyEntry
        {
            Sys = new SystemDetails { Id = "text-body-id" },
        };
        var chunk = CreateRecommendationChunk("chunk", "chunk-id", "Use MFA", "text-body-id");
        var section = CreateSection("section", chunk);
        var category = CreateCategory("category");

        _contentfulService.GetCategoryBySlugAsync("category").Returns(category);
        _contentfulService.GetSectionBySlugAsync("section").Returns(section);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);
        _currentUser.GetActiveEstablishmentNameAsync().Returns("Springfield Primary");

        _recommendationService
            .GetLatestRecommendationHistoryAsync("chunk-id", 123)
            .Returns(
                new SqlEstablishmentRecommendationHistoryDto
                {
                    NewStatus = RecommendationStatus.InProgress,
                }
            );

        _contentfulService.GetTextBodyByIdAsync("text-body-id").Returns(textBody);

        _notifyService
            .SendSingleRecommendationEmail(
                Arg.Any<ShareByEmailModel>(),
                textBody,
                "Springfield Primary",
                "Use MFA",
                "category",
                RecommendationStatus.InProgress
            )
            .Returns([new NotifySendResult { Recipient = DefaultRecipient, Errors = [] }]);

        var sut = CreateServiceUnderTest();

        var result = await sut.RouteToShareRecommendationAsync(
            controller,
            "category",
            "section",
            "chunk",
            inputModel
        );

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(
            nameof(RecommendationsController.GetSingleRecommendation),
            redirect.ActionName
        );
        Assert.Equal(
            nameof(RecommendationsController).GetControllerNameSlug(),
            redirect.ControllerName
        );

        var routeValues = Assert.IsType<IDictionary<string, object>>(
            redirect.RouteValues!,
            exactMatch: false
        );
        Assert.Equal("category", routeValues["categorySlug"]);
        Assert.Equal("section", routeValues["sectionSlug"]);
        Assert.Equal("chunk", routeValues["chunkSlug"]);

        await _recommendationService
            .Received(1)
            .GetLatestRecommendationHistoryAsync("chunk-id", 123);

        _notifyService
            .Received(1)
            .SendSingleRecommendationEmail(
                Arg.Is<ShareByEmailModel>(m =>
                    m.NameOfUser == inputModel.NameOfUser
                    && m.EmailAddresses.SequenceEqual(inputModel.EmailAddresses)
                    && m.UserMessage == inputModel.UserMessage
                ),
                textBody,
                "Springfield Primary",
                "Use MFA",
                "category",
                RecommendationStatus.InProgress
            );
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenNotifySendHasErrors_RedirectsToNotifyError()
    {
        var controller = CreateController();
        var inputModel = CreateInputModel();

        var textBody = new ComponentTextBodyEntry
        {
            Sys = new SystemDetails { Id = "text-body-id" },
        };
        var chunk = CreateRecommendationChunk("chunk", "chunk-id", "Use MFA", "text-body-id");
        var section = CreateSection("section", chunk);
        var category = CreateCategory("category");

        _contentfulService.GetCategoryBySlugAsync("category").Returns(category);
        _contentfulService.GetSectionBySlugAsync("section").Returns(section);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);
        _currentUser.GetActiveEstablishmentNameAsync().Returns("Springfield Primary");

        _recommendationService
            .GetLatestRecommendationHistoryAsync("chunk-id", 123)
            .Returns(
                new SqlEstablishmentRecommendationHistoryDto
                {
                    NewStatus = RecommendationStatus.InProgress,
                }
            );

        _contentfulService.GetTextBodyByIdAsync("text-body-id").Returns(textBody);

        _notifyService
            .SendSingleRecommendationEmail(
                Arg.Any<ShareByEmailModel>(),
                Arg.Any<ComponentTextBodyEntry>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RecommendationStatus>()
            )
            .Returns([new NotifySendResult { Recipient = DefaultRecipient, Errors = ["kaboom"] }]);

        var sut = CreateServiceUnderTest();

        var expected = PageRedirecter.RedirectToNotifyError(controller);

        var result = await sut.RouteToShareRecommendationAsync(
            controller,
            "category",
            "section",
            "chunk",
            inputModel
        );

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(expected.ActionName, redirect.ActionName);
        Assert.Equal(expected.ControllerName, redirect.ControllerName);
    }

    // ---------- UpdateRecommendationStatusAsync ----------

    [Fact]
    public async Task UpdateRecommendationStatusAsync_InvalidStatus_Sets_Error_And_Reroutes_To_SingleRecommendation()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = CreateController();

        const int establishmentId = 123;
        const string categorySlug = "cat-a";
        const string sectionSlug = "sec-1";
        const string chunkSlug = "second-chunk-2";

        _currentUser.UserId.Returns(123);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(establishmentId);

        var section = CreateSection("S1", sectionSlug, "Section One");

        _contentfulService.GetCategoryHeaderTextBySlugAsync(categorySlug).Returns("Networking");
        _contentfulService.GetSectionBySlugAsync(sectionSlug, 2).Returns(section);

        var routing = CreateRouting(
            SubmissionStatus.CompleteReviewed,
            section,
            answerSysIds: ["C1", "C2", "C3"]
        );

        _submissions
            .GetSubmissionRoutingDataAsync(
                establishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(routing);

        // Status is invalid so the extension GetRecommendationStatusEnumValue() should return null
        const string invalidStatus = "TotallyInvalidStatus";

        // History needed for RouteToSingleRecommendation
        var history1 = new SqlEstablishmentRecommendationHistoryDto
        {
            DateCreated = new DateTime(2025, 11, 14),
        };
        var history2 = new SqlEstablishmentRecommendationHistoryDto
        {
            DateCreated = new DateTime(2025, 11, 11),
        };

        _recommendationService
            .GetLatestRecommendationHistoryAsync("C2", establishmentId)
            .Returns((SqlEstablishmentRecommendationHistoryDto?)null);

        _recommendationService
            .GetRecommendationHistoryAsync("C2", establishmentId)
            .Returns([history1, history2]);

        // Act
        var result = await sut.UpdateRecommendationStatusAsync(
            ctl,
            categorySlug,
            sectionSlug,
            chunkSlug,
            invalidStatus,
            notes: null
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("SingleRecommendation", view.ViewName);

        Assert.Equal("Select a valid status", ctl.TempData["StatusUpdateError"]);

        await _recommendationService
            .DidNotReceiveWithAnyArgs()
            .UpdateRecommendationStatusAsync(
                default!,
                default,
                default,
                default!,
                default!,
                default
            );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_ValidStatus_Uses_Default_Notes_And_Sets_Success_Message()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = CreateController();

        const int establishmentId = 123;
        const int userId = 42;
        const string categorySlug = "cat-a";
        const string sectionSlug = "sec-1";
        const string chunkSlug = "second-chunk-2";

        _currentUser.GetActiveEstablishmentIdAsync().Returns(establishmentId);
        _currentUser.UserId.Returns(userId);
        _currentUser.IsMat.Returns(false);

        var section = CreateSection("S1", sectionSlug, "Section One");

        _contentfulService.GetCategoryHeaderTextBySlugAsync(categorySlug).Returns("Networking");
        _contentfulService.GetSectionBySlugAsync(sectionSlug, 2).Returns(section);

        var routing = CreateRouting(
            SubmissionStatus.CompleteReviewed,
            section,
            answerSysIds: ["C1", "C2", "C3"]
        );

        _submissions
            .GetSubmissionRoutingDataAsync(
                establishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(routing);

        // For the redirect back to RouteToSingleRecommendation
        var currentRecommendationStatus = new SqlEstablishmentRecommendationHistoryDto
        {
            EstablishmentId = establishmentId,
            RecommendationId = 2,
            UserId = userId,
            NewStatus = RecommendationStatus.NotStarted,
            DateCreated = DateTime.UtcNow.AddDays(-1),
        };

        _recommendationService
            .GetLatestRecommendationHistoryAsync("C2", establishmentId)
            .Returns(currentRecommendationStatus);

        _recommendationService
            .GetRecommendationHistoryAsync("C2", establishmentId)
            .Returns(Array.Empty<SqlEstablishmentRecommendationHistoryDto>());

        var selectedStatus = RecommendationStatus.NotStarted;
        var statusDisplayName = selectedStatus.GetDisplayName();
        var notesEntry = "Status manually updated to Not started";
        var successHeader = "Status updated to 'Not started'";

        _microcopyProvider
            .GetTextByKeyAsync(
                ContentfulMicrocopyConstants.SingleRecommendationHistoryReason,
                Arg.Is<Dictionary<string, string>>(d =>
                    d.ContainsKey("recStatus") && d["recStatus"] == statusDisplayName
                )
            )
            .Returns(notesEntry);

        _microcopyProvider
            .GetTextByKeyAsync(
                ContentfulMicrocopyConstants.SingleRecommendationSuccessHeader,
                Arg.Is<Dictionary<string, string>>(d =>
                    d != null && d.ContainsKey("recStatus") && d["recStatus"] == statusDisplayName
                )
            )
            .Returns(successHeader);

        // Act
        var result = await sut.UpdateRecommendationStatusAsync(
            ctl,
            categorySlug,
            sectionSlug,
            chunkSlug,
            selectedStatus.ToString(),
            notes: null
        );

        // Assert

        // TempData success banner should be set
        var successResult = Assert.IsType<string>(ctl.TempData["StatusUpdateSuccessTitle"]);
        Assert.Equal(successHeader, successResult);

        // Service is called with the correct ids and a default note containing our literal text
        await _recommendationService
            .Received(1)
            .UpdateRecommendationStatusAsync(
                "C2",
                establishmentId,
                userId,
                selectedStatus,
                Arg.Is<string>(n => n == notesEntry),
                Arg.Any<int?>()
            );

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_ValidStatus_Uses_Provided_Notes()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var ctl = CreateController();

        const int establishmentId = 123;
        const int userId = 99;
        const string categorySlug = "cat-a";
        const string sectionSlug = "sec-1";
        const string chunkSlug = "second-chunk-2";

        _currentUser.GetActiveEstablishmentIdAsync().Returns(establishmentId);
        _currentUser.UserId.Returns(userId);
        _currentUser.IsMat.Returns(true);
        _currentUser.UserOrganisationId.Returns(555);

        var section = CreateSection("S1", sectionSlug, "Section One");

        _contentfulService.GetCategoryHeaderTextBySlugAsync(categorySlug).Returns("Networking");
        _contentfulService.GetSectionBySlugAsync(sectionSlug, 2).Returns(section);

        var routing = CreateRouting(
            SubmissionStatus.CompleteReviewed,
            section,
            answerSysIds: ["C1", "C2", "C3"]
        );

        _submissions
            .GetSubmissionRoutingDataAsync(
                establishmentId,
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(routing);

        _recommendationService
            .GetLatestRecommendationHistoryAsync("C2", establishmentId)
            .Returns((SqlEstablishmentRecommendationHistoryDto?)null);

        _recommendationService
            .GetRecommendationHistoryAsync("C2", establishmentId)
            .Returns(Array.Empty<SqlEstablishmentRecommendationHistoryDto>());

        var selectedStatus = RecommendationStatus.NotStarted;
        const string customNotes = "This is a custom reason";

        // Act
        var result = await sut.UpdateRecommendationStatusAsync(
            ctl,
            categorySlug,
            sectionSlug,
            chunkSlug,
            selectedStatus.ToString(),
            notes: customNotes
        );

        // Assert
        await _recommendationService
            .Received(1)
            .UpdateRecommendationStatusAsync(
                "C2",
                establishmentId,
                userId,
                selectedStatus,
                customNotes,
                555
            );

        Assert.IsType<RedirectToActionResult>(result);
    }

    // ---------- Support ----------

    private static ShareByEmailInputViewModel CreateInputModel()
    {
        return new ShareByEmailInputViewModel
        {
            NameOfUser = "Drew",
            EmailAddresses = ["drew@example.com"],
            UserMessage = "Hello",
        };
    }

    private static QuestionnaireCategoryEntry CreateCategory(string heading)
    {
        return new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = heading },
        };
    }

    private static QuestionnaireSectionEntry CreateSection(
        string name,
        RecommendationChunkEntry recommendationChunk
    )
    {
        return new QuestionnaireSectionEntry
        {
            Name = name,
            CoreRecommendations = [recommendationChunk],
        };
    }

    private static RecommendationChunkEntry CreateRecommendationChunk(
        string slug,
        string id,
        string headerText,
        string textBodyId
    )
    {
        return new RecommendationChunkEntry
        {
            Slug = slug,
            Sys = new SystemDetails { Id = id },
            Header = headerText,
            Content = [new ComponentTextBodyEntry { Sys = new SystemDetails { Id = textBodyId } }],
        };
    }
}

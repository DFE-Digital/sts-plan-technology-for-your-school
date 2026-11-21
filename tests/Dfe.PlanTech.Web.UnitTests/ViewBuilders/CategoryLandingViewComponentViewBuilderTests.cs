using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class CategoryLandingViewComponentViewBuilderTests
{
    private const int UserId = 77;
    private const int FocusedEstablishmentId = 1001;
    private const int FirstGroupEstablishmentId = 101;
    private const int SecondGroupEstablishmentId = 102;
    private const int OutsideGroupEstablishmentId = 999;
    private const string CategorySlug = "networking-category";
    private const string NetworkingSectionId = "section-networking";
    private const string DevicesSectionId = "section-devices";
    private const string BroadbandSectionId = "section-broadband";
    private const string AssessmentLinkPartialName =
        "Components/CategoryLanding/SectionAssessmentLink";
    private const string AssessmentLinkPrintPartialName =
        "Components/CategoryLanding/SectionAssessmentLinkPrintContent";

    private static readonly DateTime OldestUpdate = new(2025, 1, 10, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime MiddleUpdate = new(2025, 1, 11, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime NewestUpdate = new(2025, 1, 12, 9, 0, 0, DateTimeKind.Utc);
    private static readonly Guid FirstUserActionId = new("11111111-1111-4111-8111-111111111111");
    private static readonly Guid SecondUserActionId = new("22222222-2222-4222-8222-222222222222");

    private static readonly QuestionnaireSectionEntry NetworkingSection = new()
    {
        Sys = new SystemDetails(NetworkingSectionId),
        Name = "Networking",
        InterstitialPage = new PageEntry { Slug = "networking-slug" },
    };

    private static readonly QuestionnaireSectionEntry BroadbandSection = new()
    {
        Sys = new SystemDetails(BroadbandSectionId),
        Name = "Broadband",
        InterstitialPage = new PageEntry { Slug = "broadband-slug" },
    };

    private static readonly QuestionnaireSectionEntry DevicesSection = new()
    {
        Sys = new SystemDetails(DevicesSectionId),
        Name = "Devices",
        InterstitialPage = new PageEntry { Slug = "devices-slug" },
        CoreRecommendations =
        [
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("chunk-1"),
                Header = "Devices 1",
                Slug = "devices-1",
            },
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("chunk-2"),
                Header = "Devices 2",
                Slug = "devices-2",
            },
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("chunk-3"),
                Header = "Devices 3",
                Slug = "devices-3",
            },
        ],
    };

    private static readonly QuestionnaireCategoryEntry NetworkingCategory = new()
    {
        Sys = new SystemDetails("category-networking"),
        Header = new ComponentHeaderEntry { Text = "Networking category" },
        Sections = [NetworkingSection],
    };

    private static readonly QuestionnaireCategoryEntry NetworkingAndBroadbandCategory = new()
    {
        Sys = new SystemDetails("category-networking-broadband"),
        Header = new ComponentHeaderEntry { Text = "Networking category" },
        Sections = [NetworkingSection, BroadbandSection],
    };

    private static readonly QuestionnaireCategoryEntry DevicesCategory = new()
    {
        Sys = new SystemDetails("category-devices"),
        Header = new ComponentHeaderEntry { Text = "Devices category" },
        Sections = [DevicesSection],
    };

    private readonly FakeLogger<BaseViewBuilder> _logger = new();
    private readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    private readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();
    private readonly ISubmissionService _submissionService = Substitute.For<ISubmissionService>();
    private readonly IUserActionTrackingService _userActionTrackingService =
        Substitute.For<IUserActionTrackingService>();
    private readonly IEstablishmentService _establishmentService =
        Substitute.For<IEstablishmentService>();
    private readonly IUserService _userService = Substitute.For<IUserService>();
    private readonly IGroupService _groupService = Substitute.For<IGroupService>();

    private readonly SelectedEstablishmentDataModel _firstGroupSchool =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(FirstGroupEstablishmentId)
            .Build();

    private readonly SelectedEstablishmentDataModel _secondGroupSchool =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(SecondGroupEstablishmentId)
            .Build();

    private readonly Dictionary<string, SqlEstablishmentRecommendationHistoryDto> _devicesHistory =
        new()
        {
            {
                "chunk-1",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    NewStatus = RecommendationStatus.InProgress,
                    DateCreated = MiddleUpdate,
                }
            },
            {
                "chunk-2",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    NewStatus = RecommendationStatus.Complete,
                    DateCreated = NewestUpdate,
                }
            },
            {
                "chunk-3",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    NewStatus = RecommendationStatus.NotStarted,
                    DateCreated = OldestUpdate,
                }
            },
        };

    public CategoryLandingViewComponentViewBuilderTests()
    {
        _currentUserProvider.UserId.Returns(UserId);
        _currentUserProvider
            .GetFocusedEstablishmentAsync()
            .Returns(
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FocusedEstablishmentId)
                    .Build()
            );
    }

    private CategoryLandingViewComponentViewBuilder CreateSystemUnderTest() =>
        new(
            _logger,
            _contentfulService,
            _currentUserProvider,
            _submissionService,
            _userActionTrackingService,
            _establishmentService,
            _userService,
            _groupService
        );

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        // The logger is not null-guarded in production, so it is excluded here.
        Assert.Throws<ArgumentNullException>("contentfulService", () =>
            new CategoryLandingViewComponentViewBuilder(
                _logger,
                null!,
                _currentUserProvider,
                _submissionService,
                _userActionTrackingService,
                _establishmentService,
                _userService,
                _groupService
            )
        );
        Assert.Throws<ArgumentNullException>("currentUser", () =>
            new CategoryLandingViewComponentViewBuilder(
                _logger,
                _contentfulService,
                null!,
                _submissionService,
                _userActionTrackingService,
                _establishmentService,
                _userService,
                _groupService
            )
        );
        Assert.Throws<ArgumentNullException>("submissionService", () =>
            new CategoryLandingViewComponentViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                null!,
                _userActionTrackingService,
                _establishmentService,
                _userService,
                _groupService
            )
        );
        Assert.Throws<ArgumentNullException>("userActionTrackingService", () =>
            new CategoryLandingViewComponentViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                _submissionService,
                null!,
                _establishmentService,
                _userService,
                _groupService
            )
        );
        Assert.Throws<ArgumentNullException>("establishmentService", () =>
            new CategoryLandingViewComponentViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                _submissionService,
                _userActionTrackingService,
                null!,
                _userService,
                _groupService
            )
        );
        Assert.Throws<ArgumentNullException>("userService", () =>
            new CategoryLandingViewComponentViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                _submissionService,
                _userActionTrackingService,
                _establishmentService,
                null!,
                _groupService
            )
        );
        Assert.Throws<ArgumentNullException>("groupService", () =>
            new CategoryLandingViewComponentViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                _submissionService,
                _userActionTrackingService,
                _establishmentService,
                _userService,
                null!
            )
        );
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenCategoryHasNoSections_ThrowsInvalidDataExceptionAndLogsError()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-empty"),
            Header = new ComponentHeaderEntry { Text = "Empty category" },
            Sections = [],
        };
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.BuildViewModelAsync(category, CategorySlug, null, null)
        );
        Assert.Contains("category-empty", exception.Message);
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Contains("category-empty", logRecord.Message);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenContextIsNotSupported_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            systemUnderTest.BuildViewModelAsync(
                NetworkingCategory,
                CategorySlug,
                null,
                null,
                context: (CategoryLandingContext)99
            )
        );
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSchoolContext_ReturnsCategoryDetails()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Is<IEnumerable<string>>(sectionIds =>
                    sectionIds.SequenceEqual(new[] { NetworkingSectionId })
                )
            )
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            "Networking",
            null
        );

        // Assert
        Assert.Equal("Networking category", result.CategoryName);
        Assert.Equal(CategorySlug, result.CategorySlug);
        Assert.Equal("Networking", result.SectionName);
        Assert.Same(NetworkingSection, Assert.Single(result.Sections));
        Assert.Equal(CategoryLandingContext.School, result.Context);
        Assert.False(result.IsMat);
        Assert.False(result.Print);
        Assert.Equal(AssessmentLinkPartialName, result.StatusLinkPartialName);
        Assert.Null(result.ProgressRetrievalErrorMessage);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenPrintIsRequested_ReturnsPrintStatusLinkPartial()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null,
            print: true
        );

        // Assert
        Assert.True(result.Print);
        Assert.Equal(AssessmentLinkPrintPartialName, result.StatusLinkPartialName);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSomeSectionsAreCompleted_ReturnsCompletedSectionsCount()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = NetworkingSectionId,
                    Status = SubmissionStatus.CompleteNotReviewed,
                    LastCompletionDate = NewestUpdate,
                },
                new SqlSectionStatusDto
                {
                    SectionId = BroadbandSectionId,
                    Status = SubmissionStatus.InProgress,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingAndBroadbandCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        Assert.Equal(1, result.CompletedSectionsCount);
        Assert.Collection(
            result.CategoryLandingSections,
            section =>
            {
                Assert.Equal("Networking", section.Name);
                Assert.Equal(SubmissionStatus.CompleteNotReviewed, section.ProgressStatus);
            },
            section =>
            {
                Assert.Equal("Broadband", section.Name);
                Assert.Equal(SubmissionStatus.InProgress, section.ProgressStatus);
            }
        );
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSectionHasNoStatus_ReturnsNotStartedSection()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Equal(SubmissionStatus.NotStarted, section.ProgressStatus);
        Assert.Equal("networking-slug", section.Slug);
        Assert.Null(section.Recommendations);
        Assert.Null(section.EstablishmentName);
        Assert.Equal(0, result.CompletedSectionsCount);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenProgressRetrievalFails_ReturnsErrorMessageAndRetrievalErrorSections()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .ThrowsAsync(new InvalidOperationException("database unavailable"));
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingAndBroadbandCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        Assert.Equal(
            "Unable to retrieve progress, please refresh your browser.",
            result.ProgressRetrievalErrorMessage
        );
        Assert.Equal(2, result.CategoryLandingSections.Count);
        Assert.All(result.CategoryLandingSections, section =>
            Assert.Equal(SubmissionStatus.RetrievalError, section.ProgressStatus)
        );
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Contains("database unavailable", logRecord.Message);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSectionHasNoInterstitialSlug_ReturnsRetrievalErrorSectionAndLogsError()
    {
        // Arrange
        var sectionWithoutSlug = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("section-no-slug"),
            Name = "Section without slug",
        };
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-no-slug"),
            Header = new ComponentHeaderEntry { Text = "Category without slug" },
            Sections = [sectionWithoutSlug],
        };
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(category, CategorySlug, null, null);

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Equal(SubmissionStatus.RetrievalError, section.ProgressStatus);
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Contains("section-no-slug", logRecord.Message);
    }

    [Theory]
    [InlineData("Default", RecommendationSortOrder.Default)]
    [InlineData("Last updated", RecommendationSortOrder.LastUpdated)]
    [InlineData("Status", RecommendationSortOrder.Status)]
    [InlineData("unrecognised-sort-order", RecommendationSortOrder.Default)] // Unknown values fall back to the default
    public async Task BuildViewModelAsync_WhenSortOrderIsProvided_ReturnsAndSavesSortOrder(
        string sortOrder,
        RecommendationSortOrder expectedSortOrder
    )
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            sortOrder
        );

        // Assert
        Assert.Equal(expectedSortOrder, result.SortType);
        await _userService
            .Received(1)
            .SetUserRecommendationSortOrderAsync(UserId, expectedSortOrder);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSortOrderIsNotProvided_ReturnsUsersSavedSortOrder()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([]);
        _userService
            .GetUserSettingsByUserIdAsync(UserId)
            .Returns(
                new SqlUserSettingsDto
                {
                    UserId = UserId,
                    SortOrder = RecommendationSortOrder.Status,
                }
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        Assert.Equal(RecommendationSortOrder.Status, result.SortType);
        await _userService
            .DidNotReceive()
            .SetUserRecommendationSortOrderAsync(
                Arg.Any<int>(),
                Arg.Any<RecommendationSortOrder>()
            );
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSortOrderIsNotProvidedAndUserHasNoSettings_ReturnsDefaultSortOrder()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([]);
        _userService.GetUserSettingsByUserIdAsync(UserId).Returns((SqlUserSettingsDto?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        Assert.Equal(RecommendationSortOrder.Default, result.SortType);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenCompletedSectionHasNoCoreRecommendations_ReturnsNoRecommendationsMessage()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = NetworkingSectionId,
                    Status = SubmissionStatus.CompleteReviewed,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Equal(
            "Section 'Networking' has no recommendations",
            section.Recommendations?.NoRecommendationFoundErrorMessage
        );
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenCompletedSectionHasNoInterstitialPage_ReturnsUnableToRetrieveMessage()
    {
        // Arrange
        var sectionWithoutInterstitialPage = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("section-no-interstitial"),
            Name = "Cabling",
            CoreRecommendations = DevicesSection.CoreRecommendations,
        };
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-cabling"),
            Header = new ComponentHeaderEntry { Text = "Cabling category" },
            Sections = [sectionWithoutInterstitialPage],
        };
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = "section-no-interstitial",
                    Status = SubmissionStatus.CompleteReviewed,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(category, CategorySlug, null, null);

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Equal(
            "Unable to retrieve Cabling recommendation",
            section.Recommendations?.NoRecommendationFoundErrorMessage
        );
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenCompletedSectionHasNoSubmissionResponses_ReturnsUnableToRetrieveMessage()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = DevicesSectionId,
                    Status = SubmissionStatus.CompleteReviewed,
                },
            ]);
        _submissionService
            .GetLatestSubmissionResponsesModel(
                FocusedEstablishmentId,
                DevicesSection,
                SubmissionStatus.CompleteReviewed
            )
            .Returns((SubmissionResponsesModel?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            DevicesCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Equal(
            "Unable to retrieve Devices recommendation",
            section.Recommendations?.NoRecommendationFoundErrorMessage
        );
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenRetrievingRecommendationsFails_ReturnsUnableToRetrieveMessage()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = DevicesSectionId,
                    Status = SubmissionStatus.CompleteReviewed,
                },
            ]);
        _submissionService
            .GetLatestSubmissionResponsesModel(
                FocusedEstablishmentId,
                DevicesSection,
                SubmissionStatus.CompleteReviewed
            )
            .ThrowsAsync(new DatabaseException("database unavailable"));
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            DevicesCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Equal(
            "Unable to retrieve Devices recommendation",
            section.Recommendations?.NoRecommendationFoundErrorMessage
        );
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenCompletedSectionHasRecommendations_ReturnsAnswersAndChunks()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = DevicesSectionId,
                    Status = SubmissionStatus.CompleteReviewed,
                },
            ]);
        _submissionService
            .GetLatestSubmissionResponsesModel(
                FocusedEstablishmentId,
                DevicesSection,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(
                new SubmissionResponsesModel(
                    42,
                    [
                        new QuestionWithAnswerModel
                        {
                            QuestionSysId = "question-1",
                            QuestionText = "Do you have a device policy?",
                            AnswerSysId = "answer-1",
                            AnswerText = "Yes",
                            DateCreated = OldestUpdate,
                        },
                    ]
                )
            );
        _submissionService
            .GetLatestRecommendationStatusesByEstablishmentIdAsync(FocusedEstablishmentId)
            .Returns(_devicesHistory);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            DevicesCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        var recommendations = section.Recommendations;
        Assert.NotNull(recommendations);
        Assert.Null(recommendations.NoRecommendationFoundErrorMessage);
        Assert.Equal("Devices", recommendations.SectionName);
        Assert.Equal("devices-slug", recommendations.SectionSlug);
        var answer = Assert.Single(recommendations.Answers);
        Assert.Equal("Yes", answer.AnswerText);
        Assert.Collection(
            recommendations.Chunks,
            chunk =>
            {
                Assert.Equal("Devices 1", chunk.Header);
                Assert.Equal("devices-1", chunk.Slug);
                Assert.Equal(RecommendationStatus.InProgress, chunk.Status);
                Assert.Equal(MiddleUpdate, chunk.LastUpdated);
            },
            chunk =>
            {
                Assert.Equal("Devices 2", chunk.Header);
                Assert.Equal(RecommendationStatus.Complete, chunk.Status);
                Assert.Equal(NewestUpdate, chunk.LastUpdated);
            },
            chunk =>
            {
                Assert.Equal("Devices 3", chunk.Header);
                Assert.Equal(RecommendationStatus.NotStarted, chunk.Status);
                Assert.Equal(OldestUpdate, chunk.LastUpdated);
            }
        );
    }

    [Theory]
    [InlineData("Default", new[] { "Devices 1", "Devices 2", "Devices 3" })]
    [InlineData("Status", new[] { "Devices 3", "Devices 1", "Devices 2" })]
    [InlineData("Last updated", new[] { "Devices 2", "Devices 1", "Devices 3" })]
    public async Task BuildViewModelAsync_WhenSortOrderIsProvided_ReturnsRecommendationChunksInSortedOrder(
        string sortOrder,
        string[] expectedHeaders
    )
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = DevicesSectionId,
                    Status = SubmissionStatus.CompleteReviewed,
                },
            ]);
        _submissionService
            .GetLatestSubmissionResponsesModel(
                FocusedEstablishmentId,
                DevicesSection,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(new SubmissionResponsesModel(42, []));
        _submissionService
            .GetLatestRecommendationStatusesByEstablishmentIdAsync(FocusedEstablishmentId)
            .Returns(_devicesHistory);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            DevicesCategory,
            CategorySlug,
            null,
            sortOrder
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Equal(
            expectedHeaders,
            section.Recommendations!.Chunks.Select(chunk => chunk.Header)
        );
    }

    [Theory]
    [InlineData(SubmissionStatus.InProgress)]
    [InlineData(SubmissionStatus.CompleteNotReviewed)]
    public async Task BuildViewModelAsync_WhenSectionIsNotReviewed_ReturnsEstablishmentNameFromLastUpdatedUserAction(
        SubmissionStatus status
    )
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = NetworkingSectionId,
                    Status = status,
                    LastUpdatedUserActionId = FirstUserActionId,
                    CompletedUserActionId = SecondUserActionId,
                },
            ]);
        _userActionTrackingService
            .GetAsync(FirstUserActionId)
            .Returns(new SqlUserActionDto { Id = FirstUserActionId, EstablishmentId = 2001 });
        _establishmentService
            .GetEstablishmentByIdAsync(2001)
            .Returns(
                new SqlEstablishmentDtoBuilder().WithId(2001).WithOrgName("Updating School").Build()
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Equal("Updating School", section.EstablishmentName);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSectionIsCompleteReviewed_ReturnsEstablishmentNameFromCompletedUserAction()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = NetworkingSectionId,
                    Status = SubmissionStatus.CompleteReviewed,
                    LastUpdatedUserActionId = FirstUserActionId,
                    CompletedUserActionId = SecondUserActionId,
                },
            ]);
        _userActionTrackingService
            .GetAsync(SecondUserActionId)
            .Returns(new SqlUserActionDto { Id = SecondUserActionId, EstablishmentId = 2002 });
        _establishmentService
            .GetEstablishmentByIdAsync(2002)
            .Returns(
                new SqlEstablishmentDtoBuilder()
                    .WithId(2002)
                    .WithOrgName("Completed School")
                    .Build()
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Equal("Completed School", section.EstablishmentName);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenUserActionHasTrustEstablishment_ReturnsTrustEstablishmentName()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = NetworkingSectionId,
                    Status = SubmissionStatus.InProgress,
                    LastUpdatedUserActionId = FirstUserActionId,
                },
            ]);
        _userActionTrackingService
            .GetAsync(FirstUserActionId)
            .Returns(
                new SqlUserActionDto
                {
                    Id = FirstUserActionId,
                    EstablishmentId = 2003,
                    MatEstablishmentId = 3003,
                }
            );
        _establishmentService
            .GetEstablishmentByIdAsync(3003)
            .Returns(
                new SqlEstablishmentDtoBuilder()
                    .WithId(3003)
                    .WithOrgName("Springfield Trust")
                    .Build()
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Equal("Springfield Trust", section.EstablishmentName);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenStatusHasNoRelevantUserActionId_DoesNotLookUpEstablishmentName()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = NetworkingSectionId,
                    Status = SubmissionStatus.InProgress,
                    CompletedUserActionId = SecondUserActionId,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Null(section.EstablishmentName);
        await _userActionTrackingService.DidNotReceive().GetAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenUserActionIsNotFound_DoesNotLookUpEstablishmentName()
    {
        // Arrange
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = NetworkingSectionId,
                    Status = SubmissionStatus.InProgress,
                    LastUpdatedUserActionId = FirstUserActionId,
                },
            ]);
        _userActionTrackingService.GetAsync(FirstUserActionId).Returns((SqlUserActionDto?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Null(section.EstablishmentName);
        await _establishmentService.DidNotReceive().GetEstablishmentByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenMatContext_ReturnsCategoryDetailsForGroupSchools()
    {
        // Arrange
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(
                new List<SelectedEstablishmentDataModel> { _firstGroupSchool, _secondGroupSchool }
            );
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(establishmentIds =>
                    establishmentIds.SequenceEqual(
                        new[] { FirstGroupEstablishmentId, SecondGroupEstablishmentId }
                    )
                )
            )
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            "Networking",
            null,
            context: CategoryLandingContext.MAT
        );

        // Assert
        Assert.Equal("Networking category", result.CategoryName);
        Assert.Equal(CategorySlug, result.CategorySlug);
        Assert.Equal("Networking", result.SectionName);
        Assert.Equal(CategoryLandingContext.MAT, result.Context);
        Assert.True(result.IsMat);
        Assert.Equal(AssessmentLinkPartialName, result.StatusLinkPartialName);
        Assert.Null(result.ProgressRetrievalErrorMessage);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenMatContext_DoesNotRetrieveSchoolSectionStatuses()
    {
        // Arrange
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(new List<SelectedEstablishmentDataModel> { _firstGroupSchool });
        _groupService.GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>()).Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null,
            context: CategoryLandingContext.MAT
        );

        // Assert
        await _submissionService
            .DidNotReceive()
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>());
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSomeGroupSchoolsHaveSubmitted_ReturnsRecommendationsAndOutstandingCount()
    {
        // Arrange
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(
                new List<SelectedEstablishmentDataModel> { _firstGroupSchool, _secondGroupSchool }
            );
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = FirstGroupEstablishmentId,
                    SectionId = DevicesSectionId,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            DevicesCategory,
            CategorySlug,
            null,
            null,
            context: CategoryLandingContext.MAT
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.True(section.HasSubmittedAssessments);
        Assert.True(section.HasOutstandingAssessments);
        Assert.Equal(1, section.OutstandingAssessmentCount);
        Assert.Equal(1, result.CompletedSectionsCount);
        var recommendations = section.Recommendations;
        Assert.NotNull(recommendations);
        Assert.Equal("Devices", recommendations.SectionName);
        Assert.Equal("devices-slug", recommendations.SectionSlug);
        Assert.Equal(
            new[] { "Devices 1", "Devices 2", "Devices 3" },
            recommendations.Chunks.Select(chunk => chunk.Header)
        );
        Assert.Equal(
            new[] { "devices-1", "devices-2", "devices-3" },
            recommendations.Chunks.Select(chunk => chunk.Slug)
        );
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenAllGroupSchoolsHaveSubmitted_ReturnsNoOutstandingAssessments()
    {
        // Arrange
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(
                new List<SelectedEstablishmentDataModel> { _firstGroupSchool, _secondGroupSchool }
            );
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = FirstGroupEstablishmentId,
                    SectionId = NetworkingSectionId,
                },
                new SqlSubmissionDto
                {
                    EstablishmentId = SecondGroupEstablishmentId,
                    SectionId = NetworkingSectionId,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null,
            context: CategoryLandingContext.MAT
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.True(section.HasSubmittedAssessments);
        Assert.False(section.HasOutstandingAssessments);
        Assert.Equal(0, section.OutstandingAssessmentCount);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenNoGroupSchoolHasSubmitted_ReturnsAllAssessmentsOutstandingWithoutRecommendations()
    {
        // Arrange
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(
                new List<SelectedEstablishmentDataModel> { _firstGroupSchool, _secondGroupSchool }
            );
        _groupService.GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>()).Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            DevicesCategory,
            CategorySlug,
            null,
            null,
            context: CategoryLandingContext.MAT
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.False(section.HasSubmittedAssessments);
        Assert.True(section.HasOutstandingAssessments);
        Assert.Equal(2, section.OutstandingAssessmentCount);
        Assert.Null(section.Recommendations);
        Assert.Equal(0, result.CompletedSectionsCount);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSameSchoolHasMultipleSubmissionsForSection_CountsSchoolOnce()
    {
        // Arrange
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(
                new List<SelectedEstablishmentDataModel> { _firstGroupSchool, _secondGroupSchool }
            );
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = FirstGroupEstablishmentId,
                    SectionId = NetworkingSectionId,
                },
                new SqlSubmissionDto
                {
                    EstablishmentId = FirstGroupEstablishmentId,
                    SectionId = NetworkingSectionId,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null,
            context: CategoryLandingContext.MAT
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.Equal(1, section.OutstandingAssessmentCount);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSubmissionsAreFromSchoolsOutsideGroup_IgnoresThem()
    {
        // Arrange
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(new List<SelectedEstablishmentDataModel> { _firstGroupSchool });
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = OutsideGroupEstablishmentId,
                    SectionId = NetworkingSectionId,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null,
            context: CategoryLandingContext.MAT
        );

        // Assert
        var section = Assert.Single(result.CategoryLandingSections);
        Assert.False(section.HasSubmittedAssessments);
        Assert.Equal(1, section.OutstandingAssessmentCount);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenRetrievingGroupSchoolsFails_ReturnsErrorMessageAndLogsError()
    {
        // Arrange
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .ThrowsAsync(new InvalidOperationException("group unavailable"));
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            NetworkingCategory,
            CategorySlug,
            null,
            null,
            context: CategoryLandingContext.MAT
        );

        // Assert
        Assert.Equal(CategoryLandingContext.MAT, result.Context);
        Assert.Equal(
            "Unable to retrieve recommendations, please refresh your browser.",
            result.ProgressRetrievalErrorMessage
        );
        Assert.Empty(result.CategoryLandingSections);
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Contains(CategorySlug, logRecord.Message);
    }
}

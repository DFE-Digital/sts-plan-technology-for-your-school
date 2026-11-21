using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class CategorySectionViewComponentViewBuilderTests
{
    private const int FocusedEstablishmentId = 1234;
    private const int FirstGroupEstablishmentId = 10;
    private const int SecondGroupEstablishmentId = 20;
    private const int OutsideGroupEstablishmentId = 30;
    private const string FirstSectionId = "section-1";
    private const string SecondSectionId = "section-2";

    private static readonly QuestionnaireSectionEntry FirstSection = new()
    {
        Sys = new SystemDetails(FirstSectionId),
        Name = "Section 1",
        InterstitialPage = new PageEntry { Slug = "section-1-slug" },
    };

    private static readonly QuestionnaireSectionEntry SecondSection = new()
    {
        Sys = new SystemDetails(SecondSectionId),
        Name = "Section 2",
        InterstitialPage = new PageEntry { Slug = "section-2-slug" },
    };

    private readonly FakeLogger<BaseViewBuilder> _logger = new();
    private readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    private readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();
    private readonly ISubmissionService _submissionService = Substitute.For<ISubmissionService>();
    private readonly IGroupService _groupService = Substitute.For<IGroupService>();

    private readonly SelectedEstablishmentDataModel _focusedEstablishment =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(FocusedEstablishmentId)
            .Build();

    private readonly SelectedEstablishmentDataModel _firstGroupSchool =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(FirstGroupEstablishmentId)
            .Build();

    private readonly SelectedEstablishmentDataModel _secondGroupSchool =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(SecondGroupEstablishmentId)
            .Build();

    private CategorySectionViewComponentViewBuilder CreateSystemUnderTest() =>
        new(_logger, _contentfulService, _currentUserProvider, _submissionService, _groupService);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        // The logger is not null-guarded in production, so it is excluded here.
        Assert.Throws<ArgumentNullException>("contentfulService", () =>
            new CategorySectionViewComponentViewBuilder(
                _logger,
                null!,
                _currentUserProvider,
                _submissionService,
                _groupService
            )
        );
        Assert.Throws<ArgumentNullException>("currentUser", () =>
            new CategorySectionViewComponentViewBuilder(
                _logger,
                _contentfulService,
                null!,
                _submissionService,
                _groupService
            )
        );
        Assert.Throws<ArgumentNullException>("submissionService", () =>
            new CategorySectionViewComponentViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                null!,
                _groupService
            )
        );
        Assert.Throws<ArgumentNullException>("groupService", () =>
            new CategorySectionViewComponentViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                _submissionService,
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
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            Sections = [],
        };
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.BuildViewModelAsync(category)
        );
        Assert.Contains("category-1", exception.Message);
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Contains("category-1", logRecord.Message);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenContextIsNotSupported_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            Sections = [FirstSection],
        };
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            systemUnderTest.BuildViewModelAsync(category, (CategoryLandingContext)99)
        );
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSchoolContext_ReturnsCountsSlugAndHeaderForFocusedEstablishment()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            LandingPage = new PageEntry { Slug = "networks-landing" },
            Sections = [FirstSection, SecondSection],
        };
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(_focusedEstablishment);
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Is<IEnumerable<string>>(sectionIds =>
                    sectionIds.SequenceEqual(new[] { FirstSectionId, SecondSectionId })
                )
            )
            .Returns([
                new SqlSectionStatusDto
                {
                    SectionId = FirstSectionId,
                    LastCompletionDate = new DateTime(2025, 1, 15, 9, 0, 0, DateTimeKind.Utc),
                },
                new SqlSectionStatusDto { SectionId = SecondSectionId },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(category);

        // Assert
        Assert.Equal("Networks", result.CategoryHeaderText);
        Assert.Equal("networks-landing", result.CategorySlug);
        Assert.Equal(2, result.TotalSectionCount);
        Assert.Equal(1, result.CompletedSectionCount);
        Assert.Null(result.ProgressRetrievalErrorMessage);
        Assert.Equal(CategoryLandingContext.School, result.Context);
        Assert.False(result.IsMat);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenSchoolStatusRetrievalFails_ReturnsErrorMessageAndLogsError()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            LandingPage = new PageEntry { Slug = "networks-landing" },
            Sections = [FirstSection, SecondSection],
        };
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(_focusedEstablishment);
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Throws(new InvalidOperationException("database unavailable"));
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(category);

        // Assert
        Assert.Equal(
            "Unable to retrieve progress, please refresh your browser.",
            result.ProgressRetrievalErrorMessage
        );
        Assert.Equal(0, result.CompletedSectionCount);
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Contains("database unavailable", logRecord.Message);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenCategoryHasContent_ReturnsFirstContentAsDescription()
    {
        // Arrange
        var firstContent = new MissingComponentEntry();
        var secondContent = new MissingComponentEntry();
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            LandingPage = new PageEntry { Slug = "networks-landing" },
            Sections = [FirstSection],
            Content = [firstContent, secondContent],
        };
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(_focusedEstablishment);
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(category);

        // Assert
        Assert.Same(firstContent, result.Description);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenCategoryHasNoContent_ReturnsMissingComponentAsDescription()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            LandingPage = new PageEntry { Slug = "networks-landing" },
            Sections = [FirstSection],
            Content = [],
        };
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(_focusedEstablishment);
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(category);

        // Assert
        Assert.IsType<MissingComponentEntry>(result.Description);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenLandingPageIsMissing_ReturnsNullSlugAndLogsError()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            InternalName = "Networks internal name",
            Header = new ComponentHeaderEntry { Text = "Networks" },
            LandingPage = null,
            Sections = [FirstSection],
        };
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(_focusedEstablishment);
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Any<IEnumerable<string>>()
            )
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(category);

        // Assert
        Assert.Null(result.CategorySlug);
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Contains("Networks internal name", logRecord.Message);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenMatContext_ReturnsCompletedSectionCountFromGroupSubmissions()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            LandingPage = new PageEntry { Slug = "networks-landing" },
            Sections = [FirstSection, SecondSection],
        };
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
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = FirstGroupEstablishmentId,
                    SectionId = FirstSectionId,
                },
                new SqlSubmissionDto
                {
                    EstablishmentId = SecondGroupEstablishmentId,
                    SectionId = FirstSectionId,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            category,
            CategoryLandingContext.MAT
        );

        // Assert
        Assert.Equal("Networks", result.CategoryHeaderText);
        Assert.Equal("networks-landing", result.CategorySlug);
        Assert.Equal(2, result.TotalSectionCount);
        Assert.Equal(1, result.CompletedSectionCount);
        Assert.Equal(CategoryLandingContext.MAT, result.Context);
        Assert.True(result.IsMat);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenMatHasSubmissionsForEachSection_CountsEachSectionOnce()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            LandingPage = new PageEntry { Slug = "networks-landing" },
            Sections = [FirstSection, SecondSection],
        };
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
                    SectionId = FirstSectionId,
                },
                new SqlSubmissionDto
                {
                    EstablishmentId = SecondGroupEstablishmentId,
                    SectionId = FirstSectionId,
                },
                new SqlSubmissionDto
                {
                    EstablishmentId = FirstGroupEstablishmentId,
                    SectionId = SecondSectionId,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            category,
            CategoryLandingContext.MAT
        );

        // Assert
        Assert.Equal(2, result.CompletedSectionCount);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenMatHasNoCompletedSubmissions_ReturnsZeroCompletedSections()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            Sections = [FirstSection, SecondSection],
        };
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(
                new List<SelectedEstablishmentDataModel> { _firstGroupSchool, _secondGroupSchool }
            );
        _groupService.GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>()).Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            category,
            CategoryLandingContext.MAT
        );

        // Assert
        Assert.Equal(0, result.CompletedSectionCount);
        Assert.Equal(2, result.TotalSectionCount);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenMatSubmissionsAreOutsideCategory_IgnoresThem()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            Sections = [FirstSection, SecondSection],
        };
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(new List<SelectedEstablishmentDataModel> { _firstGroupSchool });
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = FirstGroupEstablishmentId,
                    SectionId = "other-section",
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            category,
            CategoryLandingContext.MAT
        );

        // Assert
        Assert.Equal(0, result.CompletedSectionCount);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenMatSubmissionsAreFromEstablishmentsOutsideGroup_IgnoresThem()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            Sections = [FirstSection, SecondSection],
        };
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(new List<SelectedEstablishmentDataModel> { _firstGroupSchool });
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = OutsideGroupEstablishmentId,
                    SectionId = FirstSectionId,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.BuildViewModelAsync(
            category,
            CategoryLandingContext.MAT
        );

        // Assert
        Assert.Equal(0, result.CompletedSectionCount);
    }

    [Fact]
    public async Task BuildViewModelAsync_WhenMatContext_DoesNotReadFocusedEstablishment()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-1"),
            Header = new ComponentHeaderEntry { Text = "Networks" },
            Sections = [FirstSection],
        };
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(new List<SelectedEstablishmentDataModel> { _firstGroupSchool });
        _groupService.GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>()).Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.BuildViewModelAsync(category, CategoryLandingContext.MAT);

        // Assert
        await _currentUserProvider.DidNotReceive().GetFocusedEstablishmentAsync();
    }
}

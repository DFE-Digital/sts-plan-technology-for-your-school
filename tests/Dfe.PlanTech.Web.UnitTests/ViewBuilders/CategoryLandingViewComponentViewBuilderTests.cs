using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class CategoryLandingViewComponentViewBuilderTests
{
    private static CategoryLandingViewComponentViewBuilder CreateSut(
        IContentfulService? contentful = null,
        ISubmissionService? submission = null,
        ICurrentUser? currentUser = null,
        ILogger<BaseViewBuilder>? logger = null)
    {
        contentful ??= Substitute.For<IContentfulService>();
        submission ??= Substitute.For<ISubmissionService>();
        currentUser ??= Substitute.For<ICurrentUser>();

        currentUser.EstablishmentId.Returns(1001);

        logger ??= NullLogger<BaseViewBuilder>.Instance;

        return new CategoryLandingViewComponentViewBuilder(
            logger,
            contentful,
            submission,
            currentUser
        );
    }

    private static QuestionnaireCategoryEntry MakeCategory(params QuestionnaireSectionEntry[] sections) =>
        new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("cat-1"),
            Header = new ComponentHeaderEntry { Text = "Category Title" },
            Sections = sections.ToList()
        };

    private static QuestionnaireSectionEntry MakeSection(
        string id, string name, string slug, string? interstitialSlug = null) =>
        new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(id),
            Name = name,
            InterstitialPage = new PageEntry { Slug = interstitialSlug ?? slug }
        };

    // ---------- Tests ----------

    [Fact]
    public async Task BuildViewModelAsync_Throws_When_No_Sections()
    {
        // Arrange
        var category = MakeCategory(); // no sections
        var sut = CreateSut();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.BuildViewModelAsync(category, "cat-slug", null, RecommendationConstants.DefaultSortOrder));

        Assert.Contains("Found no sections", ex.Message);
    }

    [Fact]
    public async Task BuildViewModelAsync_When_Progress_Retrieval_Fails_Sets_Error_Message_And_Marks_Sections()
    {
        // Arrange
        var sectionA = MakeSection("S1", "Sec 1", "sec-1");
        var sectionB = MakeSection("S2", "Sec 2", "sec-2");
        var category = MakeCategory(sectionA, sectionB);

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Throws(new Exception("boom"));

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat-slug", "Some Section", RecommendationConstants.DefaultSortOrder);

        // Assert
        Assert.Equal("Category Title", vm.CategoryName);
        Assert.Equal("cat-slug", vm.CategorySlug);
        Assert.NotNull(vm.ProgressRetrievalErrorMessage);
        Assert.NotEmpty(vm.CategoryLandingSections);
        Assert.All(vm.CategoryLandingSections, s => Assert.Equal(SectionProgressStatus.RetrievalError, s.ProgressStatus));
    }

    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, true, false)]
    [InlineData(2, true, true)]
    public async Task BuildViewModelAsync_Computed_Completion_Flags_Are_Correct(
        int completedCount, bool anyCompleted, bool allCompleted)
    {
        // Arrange
        var s1 = MakeSection("A", "A", "a");
        var s2 = MakeSection("B", "B", "b");
        var category = MakeCategory(s1, s2);

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto
            {
                SectionId = "A",
                LastCompletionDate = completedCount >= 1 ? DateTime.UtcNow : (DateTime?)null
            },
            new SqlSectionStatusDto
            {
                SectionId = "B",
                LastCompletionDate = completedCount >= 2 ? DateTime.UtcNow : (DateTime?)null
            }
        };

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Returns(statuses);

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null, RecommendationConstants.DefaultSortOrder);

        // Assert
        Assert.Equal(anyCompleted, vm.AnySectionsCompleted);
        Assert.Equal(allCompleted, vm.AllSectionsCompleted);
    }

    [Fact]
    public async Task Recommendations_When_Intro_Null_Sets_Error_Message()
    {
        // Arrange
        var section = MakeSection("S1", "Networking", "net");
        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto { SectionId = "S1", LastMaturity = "developing" }
        };

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Returns(statuses);

        var contentful = Substitute.For<IContentfulService>();

        var sut = CreateSut(contentful: contentful, submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null, RecommendationConstants.DefaultSortOrder);

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal("Unable to retrieve Networking recommendation",
                     secVm.Recommendations.NoRecommendationFoundErrorMessage);
    }

    [Fact]
    public async Task Recommendations_When_LatestResponses_Null_Returns_Error_Message()
    {
        // Arrange
        var section = MakeSection("S1", "Security", "sec");
        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto { SectionId = "S1", LastMaturity = "established" }
        };

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Returns(statuses);

        // Force the null-coalescing throw in SUT:
        submission.GetLatestSubmissionResponsesModel(Arg.Any<int>(), section, true)
                  .Returns((SubmissionResponsesModel?)null);

        var contentful = Substitute.For<IContentfulService>();
        var sut = CreateSut(contentful: contentful, submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null, RecommendationConstants.DefaultSortOrder);

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal("Unable to retrieve Security recommendation",
                     secVm.Recommendations.NoRecommendationFoundErrorMessage);
    }

    [Fact]
    public async Task Recommendations_When_Exception_Thrown_Returns_Error_Message()
    {
        // Arrange
        var section = MakeSection("S1", "Devices", "devices");
        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto { SectionId = "S1", LastMaturity = "exemplary" }
        };

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Returns(statuses);

        var contentful = Substitute.For<IContentfulService>();

        // Throw anything inside the try block to trigger the catch path in SUT
        submission.GetLatestSubmissionResponsesModel(Arg.Any<int>(), section, true)
                  .Throws(new DatabaseException("boom"));

        var sut = CreateSut(contentful: contentful, submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null, RecommendationConstants.DefaultSortOrder);

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal("Unable to retrieve Devices recommendation",
                     secVm.Recommendations.NoRecommendationFoundErrorMessage);
    }

    [Fact]
    public async Task Recommendations_Success_Populates_Expected_Fields()
    {
        // Arrange
        var section = MakeSection("S4", "Broadband", "broadband", "broadband-connection");
        section.CoreRecommendations = new List<RecommendationChunkEntry>
        {
            new RecommendationChunkEntry { Sys = new SystemDetails("chunk-1"), Header = "Broadband" }
        };

        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "S4", LastMaturity = "developing" }
        };

        var responses = new SubmissionResponsesModel(
            submissionId: 42,
            responses: new List<QuestionWithAnswerModel>
            {
                new()
                {
                    QuestionSysId = "Q1",
                    QuestionText = "Do you have a safeguarding lead?",
                    AnswerSysId = "A1",
                    AnswerText = "Yes",
                    DateCreated = DateTime.UtcNow
                }
            });

        var recommendationHistory = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
        {
            {
                "chunk-1",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = 101,
                    RecommendationId = 201,
                    UserId = 301,
                    MatEstablishmentId = 401,
                    DateCreated = DateTime.Now,
                    PreviousStatus = "In Progress",
                    NewStatus = "Complete",
                    NoteText = "Test note"
                }
            }
        };

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);
        submission.GetLatestSubmissionResponsesModel(Arg.Any<int>(), section, true)
            .Returns(responses);
        submission.GetLatestRecommendationStatusesByRecommendationIdAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<int>())
            .Returns(recommendationHistory);

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null, RecommendationConstants.DefaultSortOrder);

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        var recs = secVm.Recommendations;
        Assert.Null(recs.NoRecommendationFoundErrorMessage);
        Assert.Equal("Broadband", recs.SectionName);
        Assert.Equal("broadband-connection", recs.SectionSlug);
        Assert.Single(recs.Answers);
        Assert.Equal("Yes", recs.Answers.First().AnswerText);
        Assert.Single(recs.Chunks);
        Assert.Equal("Complete", recs.Chunks.First().Status.GetDisplayName());
    }

    [Fact]
    public async Task Recommendations_When_CoreRecommendations_Is_Null_Returns_Error_Message()
    {
        // Arrange
        var section = MakeSection("S6", "Devices", "devices", "devices-interstitial");
        section.CoreRecommendations = null;
        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "S6", LastMaturity = "exemplary" }
        };

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Returns(statuses);
        submission.GetLatestSubmissionResponsesModel(Arg.Any<int>(), section, true)
                  .Returns(new SubmissionResponsesModel(1, new()));

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null, RecommendationConstants.DefaultSortOrder);

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal("Unable to retrieve Devices recommendation",
                     secVm.Recommendations.NoRecommendationFoundErrorMessage);
    }

    [Fact]
    public async Task Recommendations_Sorted_Correctly_When_SortOrder_Is_Status()
    {
        // Arrange
        var section = MakeSection("S6", "Devices", "devices", "devices-interstitial");
        section.CoreRecommendations = new List<RecommendationChunkEntry>
        {
            new RecommendationChunkEntry { Sys = new SystemDetails("chunk-1"), Header = "Devices 1" },
            new RecommendationChunkEntry { Sys = new SystemDetails("chunk-2"), Header = "Devices 2" },
            new RecommendationChunkEntry { Sys = new SystemDetails("chunk-3"), Header = "Devices 3" },
        };

        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "S6", LastMaturity = "developing" }
        };

        var responses = new SubmissionResponsesModel(
            submissionId: 42,
            responses: new List<QuestionWithAnswerModel>
            {
                new()
                {
                    QuestionSysId = "Q1",
                    QuestionText = "Do you have a safeguarding lead?",
                    AnswerSysId = "A1",
                    AnswerText = "Yes",
                    DateCreated = DateTime.UtcNow
                }
            });

        var recommendationHistory = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
        {
            {
                "chunk-1",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = 101,
                    RecommendationId = 201,
                    UserId = 301,
                    MatEstablishmentId = 401,
                    DateCreated = DateTime.Now,
                    PreviousStatus = RecommendationStatus.InProgress.ToString(),
                    NewStatus = RecommendationStatus.Complete.ToString(),
                    NoteText = "Test note 1"
                }
            },
            {
                "chunk-2",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = 101,
                    RecommendationId = 202,
                    UserId = 301,
                    MatEstablishmentId = 401,
                    DateCreated = DateTime.Now,
                    PreviousStatus = RecommendationStatus.NotStarted.ToString(),
                    NewStatus = RecommendationStatus.InProgress.ToString(),
                    NoteText = "Test note 2"
                }
            },
            {
                "chunk-3",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = 101,
                    RecommendationId = 204,
                    UserId = 301,
                    MatEstablishmentId = 401,
                    DateCreated = DateTime.Now,
                    PreviousStatus = null,
                    NewStatus = RecommendationStatus.NotStarted.ToString(),
                    NoteText = "Test note 3"
                }
            }
        };

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);
        submission.GetLatestSubmissionResponsesModel(Arg.Any<int>(), section, true)
            .Returns(responses);
        submission.GetLatestRecommendationStatusesByRecommendationIdAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<int>())
            .Returns(recommendationHistory);

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null, RecommendationSort.Status.GetDisplayName());

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        var chunks = secVm.Recommendations.Chunks;
        Assert.NotEmpty(chunks);
        Assert.Equal(RecommendationStatus.NotStarted, chunks[0].Status);
        Assert.Equal(RecommendationStatus.InProgress, chunks[1].Status);
        Assert.Equal(RecommendationStatus.Complete, chunks[2].Status);
    }

    [Fact]
    public async Task Recommendations_Sorted_Correctly_When_SortOrder_Is_LastUpdated()
    {
        // Arrange
        var section = MakeSection("S6", "Devices", "devices", "devices-interstitial");
        section.CoreRecommendations = new List<RecommendationChunkEntry>
        {
            new RecommendationChunkEntry { Sys = new SystemDetails("chunk-1"), Header = "Devices 1" },
            new RecommendationChunkEntry { Sys = new SystemDetails("chunk-2"), Header = "Devices 2" },
            new RecommendationChunkEntry { Sys = new SystemDetails("chunk-3"), Header = "Devices 3" },
        };

        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "S6", LastMaturity = "developing" }
        };

        var responses = new SubmissionResponsesModel(
            submissionId: 42,
            responses: new List<QuestionWithAnswerModel>
            {
                new()
                {
                    QuestionSysId = "Q1",
                    QuestionText = "Do you have a safeguarding lead?",
                    AnswerSysId = "A1",
                    AnswerText = "Yes",
                    DateCreated = DateTime.UtcNow
                }
            });

        var recommendationHistory = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
        {
            {
                "chunk-1",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = 101,
                    RecommendationId = 201,
                    UserId = 301,
                    MatEstablishmentId = 401,
                    DateCreated = DateTime.Now.AddDays(-2),
                    PreviousStatus = RecommendationStatus.InProgress.ToString(),
                    NewStatus = RecommendationStatus.Complete.ToString(),
                    NoteText = "Test note 1"
                }
            },
            {
                "chunk-2",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = 101,
                    RecommendationId = 202,
                    UserId = 301,
                    MatEstablishmentId = 401,
                    DateCreated = DateTime.Now.AddDays(-1),
                    PreviousStatus = RecommendationStatus.NotStarted.ToString(),
                    NewStatus = RecommendationStatus.InProgress.ToString(),
                    NoteText = "Test note 2"
                }
            },
            {
                "chunk-3",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = 101,
                    RecommendationId = 204,
                    UserId = 301,
                    MatEstablishmentId = 401,
                    DateCreated = DateTime.Now,
                    PreviousStatus = null,
                    NewStatus = RecommendationStatus.NotStarted.ToString(),
                    NoteText = "Test note 3"
                }
            }
        };

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);
        submission.GetLatestSubmissionResponsesModel(Arg.Any<int>(), section, true)
            .Returns(responses);
        submission.GetLatestRecommendationStatusesByRecommendationIdAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<int>())
            .Returns(recommendationHistory);

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null, RecommendationSort.LastUpdated.GetDisplayName());

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        var chunks = secVm.Recommendations.Chunks;
        Assert.NotEmpty(chunks);
        Assert.True(chunks[0].LastUpdated > chunks[1].LastUpdated && chunks[1].LastUpdated > chunks[2].LastUpdated);
    }
}

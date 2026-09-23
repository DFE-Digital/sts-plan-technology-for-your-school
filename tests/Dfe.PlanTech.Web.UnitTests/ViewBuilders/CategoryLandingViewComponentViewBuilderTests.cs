using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Models;
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
        IEstablishmentService? establishment = null,
        IUserActionTrackingService? userActionTracking = null,
        IUserService? user = null,
        IGroupService? groupService = null,
        ICurrentUserProvider? currentUser = null,
        ILogger<BaseViewBuilder>? logger = null
    )
    {
        contentful ??= Substitute.For<IContentfulService>();
        submission ??= Substitute.For<ISubmissionService>();
        establishment ??= Substitute.For<IEstablishmentService>();
        userActionTracking ??= Substitute.For<IUserActionTrackingService>();
        user ??= Substitute.For<IUserService>();
        groupService ??= Substitute.For<IGroupService>();
        currentUser ??= Substitute.For<ICurrentUserProvider>();

        currentUser.GetActiveEstablishmentIdAsync().Returns(1001);

        logger ??= NullLogger<BaseViewBuilder>.Instance;

        return new CategoryLandingViewComponentViewBuilder(
            logger,
            contentful,
            currentUser,
            submission,
            userActionTracking,
            establishment,
            user,
            groupService
        );
    }

    private static QuestionnaireCategoryEntry MakeCategory(
        params QuestionnaireSectionEntry[] sections
    ) =>
        new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("cat-1"),
            Header = new ComponentHeaderEntry { Text = "Category Title" },
            Sections = sections.ToList(),
        };

    private static QuestionnaireSectionEntry MakeSection(
        string id,
        string name,
        string slug,
        string? interstitialSlug = null
    ) =>
        new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(id),
            Name = name,
            InterstitialPage = new PageEntry { Slug = interstitialSlug ?? slug },
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
            sut.BuildViewModelAsync(
                category,
                "cat-slug",
                null,
                RecommendationConstants.DefaultSortOrder
            )
        );

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
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Throws(new Exception("boom"));

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat-slug",
            "Some Section",
            RecommendationConstants.DefaultSortOrder
        );

        // Assert
        Assert.Equal("Category Title", vm.CategoryName);
        Assert.Equal("cat-slug", vm.CategorySlug);
        Assert.NotNull(vm.ProgressRetrievalErrorMessage);
        Assert.NotEmpty(vm.CategoryLandingSections);
        Assert.All(
            vm.CategoryLandingSections,
            s => Assert.Equal(SubmissionStatus.RetrievalError, s.ProgressStatus)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task BuildViewModelAsync_Computed_Completion_Count_Correct(int completedCount)
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
                LastCompletionDate = completedCount >= 1 ? DateTime.UtcNow : (DateTime?)null,
            },
            new SqlSectionStatusDto
            {
                SectionId = "B",
                LastCompletionDate = completedCount >= 2 ? DateTime.UtcNow : (DateTime?)null,
            },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            RecommendationConstants.DefaultSortOrder
        );

        // Assert
        Assert.Equal(completedCount, vm.CompletedSectionsCount);
    }

    [Theory]
    [InlineData(null, RecommendationSortOrder.Default)]
    [InlineData("Default", RecommendationSortOrder.Default)]
    [InlineData("Last updated", RecommendationSortOrder.LastUpdated)]
    [InlineData("Status", RecommendationSortOrder.Status)]
    public async Task BuildViewModelAsync_Computed_SortType_Is_Correct(
        string? inputSortOrder,
        RecommendationSortOrder expectedSortOrder
    )
    {
        // Arrange
        var s1 = MakeSection("A", "A", "a");
        var s2 = MakeSection("B", "B", "b");
        var category = MakeCategory(s1, s2);

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto { SectionId = "A", LastCompletionDate = DateTime.UtcNow },
            new SqlSectionStatusDto { SectionId = "B", LastCompletionDate = DateTime.UtcNow },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null, inputSortOrder);

        // Assert
        Assert.Equal(expectedSortOrder, vm.SortType);
    }

    [Fact]
    public async Task Recommendations_When_Intro_Null_Sets_Error_Message()
    {
        // Arrange
        var section = MakeSection("S1", "Networking", "net");
        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto
            {
                SectionId = "S1",
                Status = SubmissionStatus.CompleteReviewed,
            },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);

        var contentful = Substitute.For<IContentfulService>();

        var sut = CreateSut(contentful: contentful, submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            section.Name,
            RecommendationConstants.DefaultSortOrder
        );

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal(
            $"Section '{section.Name}' has no recommendations",
            secVm.Recommendations?.NoRecommendationFoundErrorMessage
        );
    }

    [Fact]
    public async Task Recommendations_When_LatestResponses_Null_Returns_Error_Message()
    {
        // Arrange
        var section = MakeSection("S1", "Security", "sec");
        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto
            {
                SectionId = "S1",
                Status = SubmissionStatus.CompleteReviewed,
            },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);

        // Force the null-coalescing throw in SUT:
        submission
            .GetLatestSubmissionResponsesModel(
                Arg.Any<int>(),
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns((SubmissionResponsesModel?)null);

        var contentful = Substitute.For<IContentfulService>();
        var sut = CreateSut(contentful: contentful, submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            section.Name,
            RecommendationConstants.DefaultSortOrder
        );

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal(
            $"Section '{section.Name}' has no recommendations",
            secVm.Recommendations?.NoRecommendationFoundErrorMessage
        );
    }

    [Fact]
    public async Task Recommendations_When_Exception_Thrown_Returns_Error_Message()
    {
        // Arrange
        var section = MakeSection("S1", "Devices", "devices");
        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto
            {
                SectionId = "S1",
                Status = SubmissionStatus.CompleteReviewed,
            },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);

        var contentful = Substitute.For<IContentfulService>();

        // Throw anything inside the try block to trigger the catch path in SUT
        submission
            .GetLatestSubmissionResponsesModel(
                Arg.Any<int>(),
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Throws(new DatabaseException("boom"));

        var sut = CreateSut(contentful: contentful, submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            section.Name,
            RecommendationConstants.DefaultSortOrder
        );

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal(
            $"Section '{section.Name}' has no recommendations",
            secVm.Recommendations?.NoRecommendationFoundErrorMessage
        );
    }

    [Fact]
    public async Task Recommendations_Success_Populates_Expected_Fields()
    {
        // Arrange
        var section = MakeSection("S4", "Broadband", "broadband", "broadband-connection");
        section.CoreRecommendations = new List<RecommendationChunkEntry>
        {
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("chunk-1"),
                Header = "Broadband",
            },
        };

        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "S4", Status = SubmissionStatus.CompleteReviewed },
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
                    DateCreated = DateTime.UtcNow,
                },
            }
        );

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
                    PreviousStatus = RecommendationStatus.InProgress,
                    NewStatus = RecommendationStatus.Complete,
                    NoteText = "Test note",
                }
            },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);
        submission
            .GetLatestSubmissionResponsesModel(
                Arg.Any<int>(),
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(responses);
        submission
            .GetLatestRecommendationStatusesByEstablishmentIdAsync(Arg.Any<int>())
            .Returns(recommendationHistory);

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            RecommendationConstants.DefaultSortOrder
        );

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        var recs = secVm.Recommendations;
        Assert.NotNull(recs);
        Assert.Null(recs.NoRecommendationFoundErrorMessage);
        Assert.Equal("Broadband", recs.SectionName);
        Assert.Equal("broadband-connection", recs.SectionSlug);
        Assert.Single(recs.Answers);
        Assert.Equal("Yes", recs.Answers.First().AnswerText);
        Assert.Single(recs.Chunks);
        Assert.Equal(RecommendationStatus.Complete, recs.Chunks.First().Status);
    }

    [Fact]
    public async Task Recommendations_When_CoreRecommendations_Is_Null_Returns_Error_Message()
    {
        // Arrange
        var section = MakeSection("S6", "Devices", "devices", "devices-interstitial");
        section.CoreRecommendations = [];
        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "S6", Status = SubmissionStatus.CompleteReviewed },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);
        submission
            .GetLatestSubmissionResponsesModel(
                Arg.Any<int>(),
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(new SubmissionResponsesModel(1, []));

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            section.Name,
            RecommendationConstants.DefaultSortOrder
        );

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal(
            $"Section '{section.Name}' has no recommendations",
            secVm.Recommendations?.NoRecommendationFoundErrorMessage
        );
    }

    [Fact]
    public async Task Recommendations_Sorted_Correctly_When_SortOrder_Is_Status()
    {
        // Arrange
        var section = MakeSection("S6", "Devices", "devices", "devices-interstitial");
        section.CoreRecommendations = new List<RecommendationChunkEntry>
        {
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("chunk-1"),
                Header = "Devices 1",
            },
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("chunk-2"),
                Header = "Devices 2",
            },
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("chunk-3"),
                Header = "Devices 3",
            },
        };

        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "S6", Status = SubmissionStatus.CompleteReviewed },
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
                    DateCreated = DateTime.UtcNow,
                },
            }
        );

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
                    PreviousStatus = RecommendationStatus.InProgress,
                    NewStatus = RecommendationStatus.Complete,
                    NoteText = "Test note 1",
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
                    PreviousStatus = RecommendationStatus.NotStarted,
                    NewStatus = RecommendationStatus.InProgress,
                    NoteText = "Test note 2",
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
                    NewStatus = RecommendationStatus.NotStarted,
                    NoteText = "Test note 3",
                }
            },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);
        submission
            .GetLatestSubmissionResponsesModel(
                Arg.Any<int>(),
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(responses);
        submission
            .GetLatestRecommendationStatusesByEstablishmentIdAsync(Arg.Any<int>())
            .Returns(recommendationHistory);

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            RecommendationSortOrder.Status.GetDisplayName()
        );

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.NotNull(secVm.Recommendations);
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
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("chunk-1"),
                Header = "Devices 1",
            },
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("chunk-2"),
                Header = "Devices 2",
            },
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("chunk-3"),
                Header = "Devices 3",
            },
        };

        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "S6", Status = SubmissionStatus.CompleteReviewed },
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
                    DateCreated = DateTime.UtcNow,
                },
            }
        );

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
                    PreviousStatus = RecommendationStatus.InProgress,
                    NewStatus = RecommendationStatus.Complete,
                    NoteText = "Test note 1",
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
                    PreviousStatus = RecommendationStatus.NotStarted,
                    NewStatus = RecommendationStatus.InProgress,
                    NoteText = "Test note 2",
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
                    NewStatus = RecommendationStatus.NotStarted,
                    NoteText = "Test note 3",
                }
            },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);
        submission
            .GetLatestSubmissionResponsesModel(
                Arg.Any<int>(),
                section,
                SubmissionStatus.CompleteReviewed
            )
            .Returns(responses);
        submission
            .GetLatestRecommendationStatusesByEstablishmentIdAsync(Arg.Any<int>())
            .Returns(recommendationHistory);

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            RecommendationSortOrder.LastUpdated.GetDisplayName()
        );

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.NotNull(secVm.Recommendations);
        var chunks = secVm.Recommendations.Chunks;
        Assert.NotEmpty(chunks);
        Assert.True(
            chunks[0].LastUpdated > chunks[1].LastUpdated
            && chunks[1].LastUpdated > chunks[2].LastUpdated
        );
    }

    [Theory]
    [InlineData(SubmissionStatus.InProgress)]
    [InlineData(SubmissionStatus.CompleteNotReviewed)]
    public async Task
        BuildViewModelAsync_When_Section_InProgress_Or_CompleteNotReviewed_Uses_LastUpdatedUserActionId_To_Set_EstablishmentName(
            SubmissionStatus status
        )
    {
        // Arrange
        var section = MakeSection("S1", "Networking", "networking");
        var category = MakeCategory(section);

        var lastUpdatedUserActionId = Guid.NewGuid();

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto
            {
                SectionId = "S1",
                Status = status,
                LastUpdatedUserActionId = lastUpdatedUserActionId,
            },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);

        var userActionTracking = Substitute.For<IUserActionTrackingService>();
        userActionTracking
            .GetAsync(lastUpdatedUserActionId)
            .Returns(
                new SqlUserActionDto
                {
                    Id = lastUpdatedUserActionId,
                    EstablishmentId = 2001,
                    MatEstablishmentId = null,
                }
            );

        var establishment = Substitute.For<IEstablishmentService>();
        establishment
            .GetEstablishmentByIdAsync(2001)
            .Returns(
                new SqlEstablishmentDto
                {
                    Id = 2001,
                    OrgName = "Test School",
                }
            );

        var sut = CreateSut(
            submission: submission,
            userActionTracking: userActionTracking,
            establishment: establishment
        );

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            RecommendationConstants.DefaultSortOrder
        );

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal("Test School", secVm.EstablishmentName);

        await userActionTracking.Received(1).GetAsync(lastUpdatedUserActionId);
        await establishment.Received(1).GetEstablishmentByIdAsync(2001);
    }

    [Fact]
    public async Task
        BuildViewModelAsync_When_Section_CompleteReviewed_Uses_CompletedUserActionId_To_Set_EstablishmentName()
    {
        // Arrange
        var section = MakeSection("S1", "Cyber security", "cyber-security");
        var category = MakeCategory(section);

        var createdUserActionId = Guid.NewGuid();
        var completedUserActionId = Guid.NewGuid();

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto
            {
                SectionId = "S1",
                Status = SubmissionStatus.CompleteReviewed,
                CreatedUserActionId = createdUserActionId,
                CompletedUserActionId = completedUserActionId,
            },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);

        var userActionTracking = Substitute.For<IUserActionTrackingService>();
        userActionTracking
            .GetAsync(completedUserActionId)
            .Returns(
                new SqlUserActionDto
                {
                    Id = completedUserActionId,
                    EstablishmentId = 2002,
                    MatEstablishmentId = null,
                }
            );

        var establishment = Substitute.For<IEstablishmentService>();
        establishment
            .GetEstablishmentByIdAsync(2002)
            .Returns(
                new SqlEstablishmentDto
                {
                    Id = 2002,
                    OrgName = "Completed School",
                }
            );

        var sut = CreateSut(
            submission: submission,
            userActionTracking: userActionTracking,
            establishment: establishment
        );

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            RecommendationConstants.DefaultSortOrder
        );

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal("Completed School", secVm.EstablishmentName);

        await userActionTracking.Received(1).GetAsync(completedUserActionId);
        await userActionTracking.DidNotReceive().GetAsync(createdUserActionId);
        await establishment.Received(1).GetEstablishmentByIdAsync(2002);
    }

    [Fact]
    public async Task
        BuildViewModelAsync_When_UserAction_Has_MatEstablishmentId_Uses_MatEstablishmentId_For_EstablishmentName()
    {
        // Arrange
        var section = MakeSection("S1", "Devices", "devices");
        var category = MakeCategory(section);

        var lastUpdatedUserActionId = Guid.NewGuid();

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto
            {
                SectionId = "S1",
                Status = SubmissionStatus.InProgress,
                LastUpdatedUserActionId = lastUpdatedUserActionId,
            },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);

        var userActionTracking = Substitute.For<IUserActionTrackingService>();
        userActionTracking
            .GetAsync(lastUpdatedUserActionId)
            .Returns(
                new SqlUserActionDto
                {
                    Id = lastUpdatedUserActionId,
                    EstablishmentId = 2003,
                    MatEstablishmentId = 3003,
                }
            );

        var establishment = Substitute.For<IEstablishmentService>();
        establishment
            .GetEstablishmentByIdAsync(3003)
            .Returns(
                new SqlEstablishmentDto
                {
                    Id = 3003,
                    OrgName = "Test MAT",
                }
            );

        var sut = CreateSut(
            submission: submission,
            userActionTracking: userActionTracking,
            establishment: establishment
        );

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            RecommendationConstants.DefaultSortOrder
        );

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal("Test MAT", secVm.EstablishmentName);

        await establishment.Received(1).GetEstablishmentByIdAsync(3003);
        await establishment.DidNotReceive().GetEstablishmentByIdAsync(2003);
    }

    [Fact]
    public async Task BuildViewModelAsync_When_No_UserActionId_Does_Not_Retrieve_EstablishmentName()
    {
        // Arrange
        var section = MakeSection("S1", "Broadband", "broadband");
        var category = MakeCategory(section);

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto
            {
                SectionId = "S1",
                Status = SubmissionStatus.InProgress,
                CreatedUserActionId = null,
                CompletedUserActionId = null,
            },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);

        var userActionTracking = Substitute.For<IUserActionTrackingService>();
        var establishment = Substitute.For<IEstablishmentService>();

        var sut = CreateSut(
            submission: submission,
            userActionTracking: userActionTracking,
            establishment: establishment
        );

        // Act
        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            RecommendationConstants.DefaultSortOrder
        );

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Null(secVm.EstablishmentName);

        await userActionTracking.DidNotReceive().GetAsync(Arg.Any<Guid>());
        await establishment.DidNotReceive().GetEstablishmentByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task BuildViewModelAsync_When_Mat_No_Submissions_Sets_Outstanding_Assessments()
    {
        var section = MakeSection("S1", "Roles and responsibilities", "roles-and-responsibilities");
        section.CoreRecommendations =
        [
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("rec-1"),
                Header = "Recommendation 1",
                Slug = "recommendation-1",
            },
        ];
        var category = MakeCategory(section);

        var currentUser = Substitute.For<ICurrentUserProvider>();
        currentUser.UserOrganisationId.Returns(5001);

        var establishment = Substitute.For<IEstablishmentService>();
        establishment
            .GetEstablishmentLinks(5001)
            .Returns(
            [
                new SqlEstablishmentLinkDto { Urn = "100001" },
                new SqlEstablishmentLinkDto { Urn = "100002" },
            ]);
        establishment
            .GetEstablishmentsByReferencesAsync(
                Arg.Is<IEnumerable<string>>(urns =>
                    urns.SequenceEqual(new[] { "100001", "100002" })
                )
            )
            .Returns(
            [
                new SqlEstablishmentDto { Id = 101 },
                new SqlEstablishmentDto { Id = 102 },
            ]);

        var groupService = Substitute.For<IGroupService>();
        groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 101, 102 }))
            )
            .Returns([]);

        var submission = Substitute.For<ISubmissionService>();

        var sut = CreateSut(
            submission: submission,
            establishment: establishment,
            groupService: groupService,
            currentUser: currentUser
        );

        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            null,
            context: CategoryLandingContext.MAT
        );

        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal(CategoryLandingContext.MAT, vm.Context);
        Assert.False(secVm.HasSubmittedAssessments);
        Assert.True(secVm.HasOutstandingAssessments);
        Assert.Equal(2, secVm.OutstandingAssessmentCount);
        Assert.Null(secVm.Recommendations);
        Assert.Equal(0, vm.CompletedSectionsCount);

        await submission
            .DidNotReceive()
            .GetSectionStatusesForSchoolAsync(
                Arg.Any<int>(),
                Arg.Any<IEnumerable<string>>()
            );
    }

    [Fact]
    public async Task BuildViewModelAsync_When_Mat_Some_Schools_Have_Submitted_Populates_Recommendations()
    {
        var section = MakeSection("S1", "Roles and responsibilities", "roles-and-responsibilities");
        section.CoreRecommendations =
        [
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("rec-1"),
                Header = "Recommendation 1",
                Slug = "recommendation-1",
            },
        ];
        var category = MakeCategory(section);

        var currentUser = Substitute.For<ICurrentUserProvider>();
        currentUser.UserOrganisationId.Returns(5001);

        var establishment = Substitute.For<IEstablishmentService>();
        establishment
            .GetEstablishmentLinks(5001)
            .Returns(
            [
                new SqlEstablishmentLinkDto { Urn = "100001" },
                new SqlEstablishmentLinkDto { Urn = "100002" },
            ]);
        establishment
            .GetEstablishmentsByReferencesAsync(
                Arg.Is<IEnumerable<string>>(urns =>
                    urns.SequenceEqual(new[] { "100001", "100002" })
                )
            )
            .Returns(
            [
                new SqlEstablishmentDto { Id = 101 },
                new SqlEstablishmentDto { Id = 102 },
            ]);

        var groupService = Substitute.For<IGroupService>();
        groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 101, 102 }))
            )
            .Returns(
            [
                new SqlSubmissionDto
                {
                    Id = 1,
                    EstablishmentId = 101,
                    SectionId = "S1",
                },
            ]);

        var sut = CreateSut(
            establishment: establishment,
            groupService: groupService,
            currentUser: currentUser
        );

        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            null,
            context: CategoryLandingContext.MAT
        );

        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.True(secVm.HasSubmittedAssessments);
        Assert.True(secVm.HasOutstandingAssessments);
        Assert.Equal(1, secVm.OutstandingAssessmentCount);
        Assert.NotNull(secVm.Recommendations);
        Assert.Single(secVm.Recommendations.Chunks);
        Assert.Equal("Recommendation 1", secVm.Recommendations.Chunks[0].Header);
        Assert.Equal("recommendation-1", secVm.Recommendations.Chunks[0].Slug);
        Assert.Equal(1, vm.CompletedSectionsCount);
    }

    [Fact]
    public async Task BuildViewModelAsync_When_Mat_All_Schools_Have_Submitted_Has_No_Outstanding_Assessments()
    {
        var section = MakeSection("S1", "Roles and responsibilities", "roles-and-responsibilities");
        section.CoreRecommendations =
        [
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails("rec-1"),
                Header = "Recommendation 1",
                Slug = "recommendation-1",
            },
        ];
        var category = MakeCategory(section);

        var currentUser = Substitute.For<ICurrentUserProvider>();
        currentUser.UserOrganisationId.Returns(5001);

        var establishment = Substitute.For<IEstablishmentService>();
        establishment
            .GetEstablishmentLinks(5001)
            .Returns(
            [
                new SqlEstablishmentLinkDto { Urn = "100001" },
                new SqlEstablishmentLinkDto { Urn = "100002" },
            ]);
        establishment
            .GetEstablishmentsByReferencesAsync(
                Arg.Is<IEnumerable<string>>(urns =>
                    urns.SequenceEqual(new[] { "100001", "100002" })
                )
            )
            .Returns(
            [
                new SqlEstablishmentDto { Id = 101 },
                new SqlEstablishmentDto { Id = 102 },
            ]);

        var groupService = Substitute.For<IGroupService>();
        groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 101, 102 }))
            )
            .Returns(
            [
                new SqlSubmissionDto
                {
                    Id = 1,
                    EstablishmentId = 101,
                    SectionId = "S1",
                },
                new SqlSubmissionDto
                {
                    Id = 2,
                    EstablishmentId = 102,
                    SectionId = "S1",
                },
            ]);

        var sut = CreateSut(
            establishment: establishment,
            groupService: groupService,
            currentUser: currentUser
        );

        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            null,
            context: CategoryLandingContext.MAT
        );

        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.True(secVm.HasSubmittedAssessments);
        Assert.False(secVm.HasOutstandingAssessments);
        Assert.Equal(0, secVm.OutstandingAssessmentCount);
        Assert.NotNull(secVm.Recommendations);
        Assert.Equal(1, vm.CompletedSectionsCount);
    }

    [Fact]
    public async Task BuildViewModelAsync_When_Mat_Retrieval_Fails_Sets_Error_Message()
    {
        var section = MakeSection("S1", "Roles and responsibilities", "roles-and-responsibilities");
        var category = MakeCategory(section);

        var currentUser = Substitute.For<ICurrentUserProvider>();
        currentUser.UserOrganisationId.Returns(5001);

        var establishment = Substitute.For<IEstablishmentService>();
        establishment
            .GetEstablishmentLinks(5001)
            .Throws(new Exception("boom"));

        var groupService = Substitute.For<IGroupService>();

        var sut = CreateSut(
            establishment: establishment,
            groupService: groupService,
            currentUser: currentUser
        );

        var vm = await sut.BuildViewModelAsync(
            category,
            "cat",
            null,
            null,
            context: CategoryLandingContext.MAT
        );

        Assert.Equal(CategoryLandingContext.MAT, vm.Context);
        Assert.Equal(
            "Unable to retrieve recommendations, please refresh your browser.",
            vm.ProgressRetrievalErrorMessage
        );
        Assert.Empty(vm.CategoryLandingSections);

        await groupService
            .DidNotReceive()
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>());
    }

}

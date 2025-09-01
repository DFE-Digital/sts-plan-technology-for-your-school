using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context;
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
            sut.BuildViewModelAsync(category, "cat-slug", null));

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
        var vm = await sut.BuildViewModelAsync(category, "cat-slug", "Some Section");

        // Assert
        Assert.Equal("Category Title", vm.CategoryName);
        Assert.Equal("cat-slug", vm.CategorySlug);
        Assert.NotNull(vm.ProgressRetrievalErrorMessage);
        Assert.NotEmpty(vm.CategoryLandingSections);
        Assert.All(vm.CategoryLandingSections, s => Assert.True(s.ProgressStatus == SectionProgressStatus.RetrievalError));
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
        var vm = await sut.BuildViewModelAsync(category, "cat", null);

        // Assert
        Assert.Equal(anyCompleted, vm.AnySectionsCompleted);
        Assert.Equal(allCompleted, vm.AllSectionsCompleted);
    }

    [Fact]
    public async Task Recommendations_When_LastMaturity_Missing_Returns_Empty_Model()
    {
        // Arrange
        var section = MakeSection("S1", "Sec 1", "sec-1");
        var category = MakeCategory(section);

        // No maturity info -> section status without LastMaturity
        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto { SectionId = "S1", LastMaturity = null }
        };

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Returns(statuses);

        var sut = CreateSut(submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null);

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Null(secVm.Recommendations.SectionName);
        Assert.Null(secVm.Recommendations.SectionSlug);
        Assert.Null(secVm.Recommendations.NoRecommendationFoundErrorMessage);
        Assert.Null(secVm.Recommendations.Answers);
        Assert.Null(secVm.Recommendations.Chunks);
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
        // Intro not found -> should surface error
        contentful.GetSubtopicRecommendationIntroAsync("S1", "developing")
                  .Returns((RecommendationIntroEntry?)null);

        var sut = CreateSut(contentful: contentful, submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null);

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
        // Intro exists so we get to the answers stage
        contentful.GetSubtopicRecommendationIntroAsync("S1", "established")
                  .Returns(new RecommendationIntroEntry { Sys = new SystemDetails("intro-1") });

        // Subtopic recommendation exists (we won't reach its chunks because the answers are null)
        contentful.GetSubtopicRecommendationByIdAsync("S1")
                  .Returns(new SubtopicRecommendationEntry
                  {
                      Sys = new SystemDetails("rec-1"),
                      Section = new RecommendationSectionEntry() // simple placeholder object
                  });

        var sut = CreateSut(contentful: contentful, submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null);

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
        contentful.GetSubtopicRecommendationIntroAsync("S1", "exemplary")
                  .Returns(new RecommendationIntroEntry { Sys = new SystemDetails("intro-xyz") });

        // Throw anything inside the try block to trigger the catch path in SUT
        submission.GetLatestSubmissionResponsesModel(Arg.Any<int>(), section, true)
                  .Throws(new DatabaseException("boom"));

        var sut = CreateSut(contentful: contentful, submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category, "cat", null);

        // Assert
        var secVm = Assert.Single(vm.CategoryLandingSections);
        Assert.Equal("Unable to retrieve Devices recommendation",
                     secVm.Recommendations.NoRecommendationFoundErrorMessage);
    }
}

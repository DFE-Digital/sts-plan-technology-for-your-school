using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class GroupsDashboardViewComponentViewBuilderTests
{
    // ---------- helpers ----------
    private static GroupsDashboardViewComponentViewBuilder CreateServiceUnderTest(
        ILogger<BaseViewBuilder>? logger = null,
        IContentfulService? contentful = null,
        IEstablishmentService? establishments = null,
        ISubmissionService? submission = null,
        ICurrentUser? currentUser = null,
        bool addGroupSelectedSchoolUrn = true,
        bool addLatestSelectedGroupSchoolAsync = true
    )
    {
        contentful ??= Substitute.For<IContentfulService>();
        establishments ??= Substitute.For<IEstablishmentService>();
        submission ??= Substitute.For<ISubmissionService>();
        currentUser ??= Substitute.For<ICurrentUser>();
        logger ??= NullLogger<BaseViewBuilder>.Instance;

        if (addGroupSelectedSchoolUrn)
        {
            currentUser.GroupSelectedSchoolUrn.Returns("URN-123");
        }

        if (addLatestSelectedGroupSchoolAsync)
        {
            establishments.GetLatestSelectedGroupSchoolAsync("URN-123")
                .Returns(new SqlEstablishmentDto { Id = 42, EstablishmentRef = "URN-123", OrgName = "Selected School" });
        }

        return new GroupsDashboardViewComponentViewBuilder(
            logger, contentful, establishments, submission, currentUser);
    }

    private static QuestionnaireSectionEntry MakeSection(string id, string name = "Section", string slug = "sec")
        => new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(id),
            Name = name,
            InterstitialPage = new PageEntry { Slug = slug }
        };

    private static QuestionnaireCategoryEntry MakeCategory(
        IEnumerable<QuestionnaireSectionEntry> sections,
        List<ContentfulEntry>? content = null)
        => new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("cat-1"),
            Header = new ComponentHeaderEntry { Text = "Category Header" },
            Sections = sections.ToList(),
            Content = content
        };

    // ---------- tests ----------

    [Fact]
    public async Task BuildViewModelAsync_Throws_When_No_Sections()
    {
        var sut = CreateServiceUnderTest();
        var category = MakeCategory(Array.Empty<QuestionnaireSectionEntry>());

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.BuildViewModelAsync(category));

        Assert.Contains("No sections found for category", ex.Message);
    }

    [Fact]
    public async Task BuildViewModelAsync_Throws_When_GroupSelectedSchoolUrn_Is_Null()
    {
        var submissionService = Substitute.For<ISubmissionService>();
        var establishments = Substitute.For<IEstablishmentService>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.GroupSelectedSchoolUrn.Returns((string?)null);

        var sut = CreateServiceUnderTest(establishments: establishments, submission: submissionService, currentUser: currentUser, addGroupSelectedSchoolUrn: false);

        var category = MakeCategory([MakeSection("S1")]);

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.BuildViewModelAsync(category));

        Assert.Contains("GroupSelectedSchoolUrn is null", ex.Message);
        await establishments.DidNotReceiveWithAnyArgs().GetLatestSelectedGroupSchoolAsync(default!);
        await submissionService.DidNotReceiveWithAnyArgs().GetSectionStatusesForSchoolAsync(default, default!);
    }

    [Fact]
    public async Task BuildViewModelAsync_Success_Returns_ViewModel_With_Sections_And_No_Error()
    {
        // Arrange
        var s1 = MakeSection("S1", "Networking", "net");
        var s2 = MakeSection("S2", "Security", "sec");
        var category = MakeCategory(new[] { s1, s2 }, content: new List<ContentfulEntry> { new MissingComponentEntry() });

        var submission = Substitute.For<ISubmissionService>();
        // Return statuses with null maturity to avoid deeper recommendation lookups
        submission.GetSectionStatusesForSchoolAsync(42, Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { "S1", "S2" })))
                  .Returns(new List<SqlSectionStatusDto>
                  {
                      new SqlSectionStatusDto { SectionId = "S1", Completed = true,  LastMaturity = null },
                      new SqlSectionStatusDto { SectionId = "S2", Completed = false, LastMaturity = null }
                  });

        var establishments = Substitute.For<IEstablishmentService>();
        establishments.GetLatestSelectedGroupSchoolAsync("URN-123")
            .Returns(new SqlEstablishmentDto { Id = 42, EstablishmentRef = "URN-123", OrgName = "Selected School" });

        var sut = CreateServiceUnderTest(establishments: establishments, submission: submission);

        // Act
        var vm = await sut.BuildViewModelAsync(category);

        // Assert
        Assert.NotNull(vm);
        Assert.Null(vm.ProgressRetrievalErrorMessage);
        Assert.NotNull(vm.Description); // first content item (MissingComponentEntry we provided)
        Assert.Equal(2, vm.GroupsCategorySections.Count);

        // On success: hadRetrievalError should be FALSE
        Assert.All(vm.GroupsCategorySections, x => Assert.NotEqual("Unable to retrieve status", x.Tag.Text));

        await establishments.Received(1).GetLatestSelectedGroupSchoolAsync("URN-123");
        await submission.Received(1).GetSectionStatusesForSchoolAsync(
            42, Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { "S1", "S2" })));
    }

    [Fact]
    public async Task BuildViewModelAsync_Description_Falls_Back_To_MissingComponent_When_No_Content()
    {
        var category = MakeCategory(new[] { MakeSection("S1") }, content: new List<ContentfulEntry>());

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(42, Arg.Any<IEnumerable<string>>())
                  .Returns(new List<SqlSectionStatusDto>());

        var sut = CreateServiceUnderTest(submission: submission);

        var vm = await sut.BuildViewModelAsync(category);

        Assert.IsType<MissingComponentEntry>(vm.Description);
    }

    [Fact]
    public async Task BuildViewModelAsync_When_Status_Retrieval_Fails_Sets_Error_And_Flag_On_Children()
    {
        var s1 = MakeSection("S1");
        var s2 = MakeSection("S2");
        var category = MakeCategory([s1, s2]);

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(42, Arg.Any<IEnumerable<string>>())
                  .Throws(new Exception("boom"));

        var sut = CreateServiceUnderTest(submission: submission);

        var vm = await sut.BuildViewModelAsync(category);

        Assert.Equal("Unable to retrieve progress, please refresh your browser.", vm.ProgressRetrievalErrorMessage);
        Assert.Equal(2, vm.GroupsCategorySections.Count);
        // On failure: hadRetrievalError should be TRUE
        Assert.All(vm.GroupsCategorySections, x => Assert.Equal("Unable to retrieve status", x.Tag.Text));
    }
}

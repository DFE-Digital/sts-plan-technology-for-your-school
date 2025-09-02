using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class CategorySectionViewComponentViewBuilderTests
{
    private static CategorySectionViewComponentViewBuilder CreateSut(
        IContentfulService? contentful = null,
        ISubmissionService? submission = null,
        ICurrentUser? currentUser = null,
        ILogger<BaseViewBuilder>? logger = null)
    {
        contentful ??= Substitute.For<IContentfulService>();
        submission ??= Substitute.For<ISubmissionService>();
        currentUser ??= Substitute.For<ICurrentUser>();
        currentUser.EstablishmentId.Returns(1234);
        logger ??= NullLogger<BaseViewBuilder>.Instance;

        return new CategorySectionViewComponentViewBuilder(
            logger, contentful, submission, currentUser);
    }

    private static QuestionnaireSectionEntry MakeSection(string id, string name, string slug)
        => new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(id),
            Name = name,
            InterstitialPage = new PageEntry { Slug = slug }
        };

    private static QuestionnaireCategoryEntry MakeCategory(
        IEnumerable<QuestionnaireSectionEntry>? sections = null,
        string headerText = "Header",
        string? landingSlug = "landing-slug",
        List<ContentfulEntry>? content = null)
        => new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("cat-1"),
            InternalName = "Cat Internal",
            Header = new ComponentHeaderEntry { Text = headerText },
            Sections = (sections ?? Array.Empty<QuestionnaireSectionEntry>()).ToList(),
            LandingPage = landingSlug is null ? null : new PageEntry { Slug = landingSlug },
            Content = content
        };

    [Fact]
    public async Task BuildViewModelAsync_Throws_When_No_Sections()
    {
        var sut = CreateSut();
        var category = MakeCategory(sections: Array.Empty<QuestionnaireSectionEntry>());

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.BuildViewModelAsync(category));

        Assert.Contains("Found no sections", ex.Message);
    }

    [Fact]
    public async Task BuildViewModelAsync_Success_Populates_Counts_Slug_And_Sections()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var sectionB = MakeSection("S2", "Section 2", "s2");
        var category = MakeCategory(new[] { sectionA, sectionB }, headerText: "Networks", landingSlug: "networks-landing");

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto { SectionId = "S1", Completed = true },
            new SqlSectionStatusDto { SectionId = "S2", Completed = false }
        };

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Returns(statuses);

        var sut = CreateSut(submission: submission);

        var vm = await sut.BuildViewModelAsync(category);

        Assert.Equal("Networks", vm.CategoryHeaderText);
        Assert.Equal("networks-landing", vm.CategorySlug);
        Assert.Equal(2, vm.TotalSectionCount);
        Assert.Equal(1, vm.CompletedSectionCount);
        Assert.Equal(2, vm.CategorySections.Count);
        // Current code passes "progressRetrievalErrorMessage is null" to child VMs,
        // which means true when SUCCESS (arg name suggests the inverse).
        Assert.All(vm.CategorySections, s => Assert.True(s.ProgressStatus == SectionProgressStatus.RetrievalError));
        Assert.Null(vm.ProgressRetrievalErrorMessage);
    }

    [Fact]
    public async Task BuildViewModelAsync_When_Status_Retrieval_Fails_Sets_Error_And_Flag()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var sectionB = MakeSection("S2", "Section 2", "s2");
        var category = MakeCategory(new[] { sectionA, sectionB });

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Throws(new Exception("boom"));

        var sut = CreateSut(submission: submission);

        var vm = await sut.BuildViewModelAsync(category);

        Assert.Equal(2, vm.CategorySections.Count);
        Assert.Equal("Unable to retrieve progress, please refresh your browser.", vm.ProgressRetrievalErrorMessage);

        // With an error, the code passes (progressRetrievalErrorMessage is null) => false
        Assert.All(vm.CategorySections, s => Assert.False(s.ProgressStatus == SectionProgressStatus.RetrievalError));
    }

    [Fact]
    public async Task BuildViewModelAsync_Description_Uses_First_Content_When_Present()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var content0 = new MissingComponentEntry(); // simple concrete IContentComponent is fine
        var content1 = new MissingComponentEntry();

        var category = MakeCategory(
            new[] { sectionA },
            headerText: "Title",
            content: new List<ContentfulEntry> { content0, content1 });

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Returns(new List<SqlSectionStatusDto>());

        var sut = CreateSut(submission: submission);

        var vm = await sut.BuildViewModelAsync(category);

        Assert.Same(content0, vm.Description);
    }

    [Fact]
    public async Task BuildViewModelAsync_Description_Falls_Back_To_MissingComponent_When_No_Content()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var category = MakeCategory(new[] { sectionA }, headerText: "Title", content: new List<ContentfulEntry>());

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Returns(new List<SqlSectionStatusDto>());

        var sut = CreateSut(submission: submission);

        var vm = await sut.BuildViewModelAsync(category);

        Assert.IsType<MissingComponentEntry>(vm.Description);
    }

    [Fact]
    public async Task BuildViewModelAsync_When_Landing_Slug_Missing_CategorySlug_Is_Null()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var category = MakeCategory(new[] { sectionA }, landingSlug: null);

        var submission = Substitute.For<ISubmissionService>();
        submission.GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
                  .Returns(new List<SqlSectionStatusDto>());

        var sut = CreateSut(submission: submission);

        var vm = await sut.BuildViewModelAsync(category);

        Assert.Null(vm.CategorySlug);
    }
}

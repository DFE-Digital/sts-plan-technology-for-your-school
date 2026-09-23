using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
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
        ICurrentUserProvider? currentUser = null,
        IEstablishmentService? establishmentService = null,
        IGroupService? groupService = null,
        ILogger<BaseViewBuilder>? logger = null
    )
    {
        contentful ??= Substitute.For<IContentfulService>();
        submission ??= Substitute.For<ISubmissionService>();
        currentUser ??= Substitute.For<ICurrentUserProvider>();
        establishmentService ??= Substitute.For<IEstablishmentService>();
        groupService ??= Substitute.For<IGroupService>();
        currentUser.GetActiveEstablishmentIdAsync().Returns(1234);
        currentUser.UserOrganisationId.Returns(100);
        logger ??= NullLogger<BaseViewBuilder>.Instance;

        return new CategorySectionViewComponentViewBuilder(
            logger,
            contentful,
            currentUser,
            submission,
            establishmentService,
            groupService
        );
    }

    private static QuestionnaireSectionEntry MakeSection(string id, string name, string slug) =>
        new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(id),
            Name = name,
            InterstitialPage = new PageEntry { Slug = slug },
        };

    private static QuestionnaireCategoryEntry MakeCategory(
        IEnumerable<QuestionnaireSectionEntry>? sections = null,
        string headerText = "Header",
        string? landingSlug = "landing-slug",
        List<ContentfulEntry>? content = null
    ) =>
        new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("cat-1"),
            InternalName = "Cat Internal",
            Header = new ComponentHeaderEntry { Text = headerText },
            Sections = (sections ?? Array.Empty<QuestionnaireSectionEntry>()).ToList(),
            LandingPage = landingSlug is null ? null : new PageEntry { Slug = landingSlug },
            Content = content,
        };

    [Fact]
    public async Task BuildViewModelAsync_Throws_When_No_Sections()
    {
        var sut = CreateSut();
        var category = MakeCategory(sections: Array.Empty<QuestionnaireSectionEntry>());

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.BuildViewModelAsync(category)
        );

        Assert.Contains("Found no sections", ex.Message);
    }

    [Fact]
    public async Task BuildViewModelAsync_Success_Populates_Counts_Slug_And_Sections()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var sectionB = MakeSection("S2", "Section 2", "s2");
        var category = MakeCategory(
            new[] { sectionA, sectionB },
            headerText: "Networks",
            landingSlug: "networks-landing"
        );

        var statuses = new List<SqlSectionStatusDto>
        {
            new SqlSectionStatusDto { SectionId = "S1", LastCompletionDate = new DateTime() },
            new SqlSectionStatusDto { SectionId = "S2" },
        };

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(statuses);

        var sut = CreateSut(submission: submission);

        var vm = await sut.BuildViewModelAsync(category);

        Assert.Equal("Networks", vm.CategoryHeaderText);
        Assert.Equal("networks-landing", vm.CategorySlug);
        Assert.Equal(2, vm.TotalSectionCount);
        Assert.Equal(1, vm.CompletedSectionCount);
        Assert.Null(vm.ProgressRetrievalErrorMessage);
        Assert.Equal(CategoryLandingContext.School, vm.Context);
        Assert.False(vm.IsMat);
    }

    [Fact]
    public async Task BuildViewModelAsync_When_Status_Retrieval_Fails_Sets_Error_And_Flag()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var sectionB = MakeSection("S2", "Section 2", "s2");
        var category = MakeCategory(new[] { sectionA, sectionB });

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Throws(new Exception("boom"));

        var sut = CreateSut(submission: submission);

        var vm = await sut.BuildViewModelAsync(category);

        Assert.Equal(
            "Unable to retrieve progress, please refresh your browser.",
            vm.ProgressRetrievalErrorMessage
        );
    }

    [Fact]
    public async Task BuildViewModelAsync_Description_Uses_First_Content_When_Present()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var content0 = new MissingComponentEntry();
        var content1 = new MissingComponentEntry();

        var category = MakeCategory(
            new[] { sectionA },
            headerText: "Title",
            content: new List<ContentfulEntry> { content0, content1 }
        );

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(new List<SqlSectionStatusDto>());

        var sut = CreateSut(submission: submission);

        var vm = await sut.BuildViewModelAsync(category);

        Assert.Same(content0, vm.Description);
    }

    [Fact]
    public async Task BuildViewModelAsync_Description_Falls_Back_To_MissingComponent_When_No_Content()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var category = MakeCategory(
            new[] { sectionA },
            headerText: "Title",
            content: new List<ContentfulEntry>()
        );

        var submission = Substitute.For<ISubmissionService>();
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
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
        submission
            .GetSectionStatusesForSchoolAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>())
            .Returns(new List<SqlSectionStatusDto>());

        var sut = CreateSut(submission: submission);

        var vm = await sut.BuildViewModelAsync(category);

        Assert.Null(vm.CategorySlug);
    }

    [Fact]
    public async Task BuildViewModelAsync_Mat_Populates_Counts_From_Group_Submissions()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var sectionB = MakeSection("S2", "Section 2", "s2");
        var category = MakeCategory(
            new[] { sectionA, sectionB },
            headerText: "Networks",
            landingSlug: "networks-landing"
        );

        var establishmentService = Substitute.For<IEstablishmentService>();
        var groupService = Substitute.For<IGroupService>();

        establishmentService
            .GetEstablishmentLinks(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                    new() { Urn = "URN-1" },
                    new() { Urn = "URN-2" },
                }
            );

        establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<string[]>())
            .Returns(
                new List<SqlEstablishmentDto>
                {
                    new() { Id = 10 },
                    new() { Id = 20 },
                }
            );

        groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns(
                new List<SqlSubmissionDto>
                {
                    new()
                    {
                        Id = 1,
                        EstablishmentId = 10,
                        SectionId = "S1",
                    },
                    new()
                    {
                        Id = 2,
                        EstablishmentId = 20,
                        SectionId = "S1",
                    },
                }
            );

        var sut = CreateSut(
            establishmentService: establishmentService,
            groupService: groupService
        );

        var vm = await sut.BuildViewModelAsync(
            category,
            CategoryLandingContext.MAT
        );

        Assert.Equal("Networks", vm.CategoryHeaderText);
        Assert.Equal("networks-landing", vm.CategorySlug);
        Assert.Equal(2, vm.TotalSectionCount);
        Assert.Equal(1, vm.CompletedSectionCount);
        Assert.Equal(CategoryLandingContext.MAT, vm.Context);
        Assert.True(vm.IsMat);

        await establishmentService
            .Received(1)
            .GetEstablishmentLinks(100);

        await establishmentService
            .Received(1)
            .GetEstablishmentsByReferencesAsync(
                Arg.Is<string[]>(urns =>
                    urns.SequenceEqual(new[] { "URN-1", "URN-2" })
                )
            );

        await groupService
            .Received(1)
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids =>
                    ids.SequenceEqual(new[] { 10, 20 })
                )
            );
    }

    [Fact]
    public async Task BuildViewModelAsync_Mat_With_No_Completed_Submissions_Has_Zero_Completed_Sections()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var sectionB = MakeSection("S2", "Section 2", "s2");
        var category = MakeCategory(new[] { sectionA, sectionB });

        var establishmentService = Substitute.For<IEstablishmentService>();
        var groupService = Substitute.For<IGroupService>();

        establishmentService
            .GetEstablishmentLinks(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                    new() { Urn = "URN-1" },
                    new() { Urn = "URN-2" },
                }
            );

        establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<string[]>())
            .Returns(
                new List<SqlEstablishmentDto>
                {
                    new() { Id = 10 },
                    new() { Id = 20 },
                }
            );

        groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns(new List<SqlSubmissionDto>());

        var sut = CreateSut(
            establishmentService: establishmentService,
            groupService: groupService
        );

        var vm = await sut.BuildViewModelAsync(
            category,
            CategoryLandingContext.MAT
        );

        Assert.Equal(0, vm.CompletedSectionCount);
        Assert.Equal(2, vm.TotalSectionCount);
        Assert.Equal(CategoryLandingContext.MAT, vm.Context);
        Assert.True(vm.IsMat);
    }

    [Fact]
    public async Task BuildViewModelAsync_Mat_Ignores_Submissions_Outside_Category()
    {
        var sectionA = MakeSection("S1", "Section 1", "s1");
        var sectionB = MakeSection("S2", "Section 2", "s2");
        var category = MakeCategory(new[] { sectionA, sectionB });

        var establishmentService = Substitute.For<IEstablishmentService>();
        var groupService = Substitute.For<IGroupService>();

        establishmentService
            .GetEstablishmentLinks(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                    new() { Urn = "URN-1" },
                }
            );

        establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<string[]>())
            .Returns(
                new List<SqlEstablishmentDto>
                {
                    new() { Id = 10 },
                }
            );

        groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns(
                new List<SqlSubmissionDto>
                {
                    new()
                    {
                        Id = 1,
                        EstablishmentId = 10,
                        SectionId = "OTHER-SECTION",
                    },
                }
            );

        var sut = CreateSut(
            establishmentService: establishmentService,
            groupService: groupService
        );

        var vm = await sut.BuildViewModelAsync(
            category,
            CategoryLandingContext.MAT
        );

        Assert.Equal(0, vm.CompletedSectionCount);
    }
}

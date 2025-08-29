using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Options;
using Dfe.PlanTech.Core.Contentful.Options;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Data.Contentful.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class ContentfulWorkflowTests
{
    private readonly ILogger<ContentfulWorkflow> _logger = Substitute.For<ILogger<ContentfulWorkflow>>();
    private readonly IContentfulRepository _repo = Substitute.For<IContentfulRepository>();
    private readonly GetPageFromContentfulOptions _pageOpts = new() { Include = 4 };

    private ContentfulWorkflow CreateServiceUnderTest() => new(_logger, _repo, _pageOpts);

    [Fact]
    public void Ctor_NullRepository_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ContentfulWorkflow(_logger, null!, _pageOpts));
    }

    [Fact]
    public void Ctor_NullPageOptions_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ContentfulWorkflow(_logger, _repo, null!));
    }

    [Fact]
    public async Task GetEntryById_Returns_Entry()
    {
        var sut = CreateServiceUnderTest();
        var id = "P1";
        var entry = new PageEntry { Sys = new SystemDetails(id) };
        _repo.GetEntryByIdAsync<PageEntry>("p1").Returns(entry);

        var result = await sut.GetEntryById<PageEntry>("p1");

        Assert.Same(entry, result);
        await _repo.Received(1).GetEntryByIdAsync<PageEntry>("p1");
    }

    [Fact]
    public async Task GetEntryById_When_Null_Throws_Wrapped_And_Logs()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntryByIdAsync<PageEntry>("missing").Returns((PageEntry?)null);

        var ex = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(
            () => sut.GetEntryById<PageEntry>("missing"));

        Assert.Contains("missing", ex.Message);
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }

    [Fact]
    public async Task GetEntryById_When_RepoThrows_Wraps_And_Logs()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntryByIdAsync<PageEntry>("x").Returns<Task<PageEntry?>>(_ => throw new InvalidOperationException("boom"));

        var ex = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(
            () => sut.GetEntryById<PageEntry>("x"));

        Assert.Contains("x", ex.Message);
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }

    // ---------- GetEntries<T> ----------
    [Fact]
    public async Task GetEntries_Returns_List()
    {
        var sut = CreateServiceUnderTest();
        var list = new List<NavigationLinkEntry> { new() { Sys = new SystemDetails("N1") } };
        _repo.GetEntriesAsync<NavigationLinkEntry>().Returns(list);

        var result = await sut.GetEntries<NavigationLinkEntry>();

        Assert.Equal(list, result);
        await _repo.Received(1).GetEntriesAsync<NavigationLinkEntry>();
    }

    [Fact]
    public async Task GetEntries_When_Empty_Throws_Wrapped_And_Logs()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<NavigationLinkEntry>(Arg.Any<GetEntriesOptions>())
             .Returns([]);

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() => sut.GetEntries<NavigationLinkEntry>());
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }

    [Fact]
    public async Task GetEntries_When_RepoThrows_Wraps_And_Logs()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<NavigationLinkEntry>(Arg.Any<GetEntriesOptions>())
             .Returns<Task<IEnumerable<NavigationLinkEntry>>>(_ => throw new Exception("boom"));

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() => sut.GetEntries<NavigationLinkEntry>());
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }

    // ---------- GetAllSectionsAsync ----------
    [Fact]
    public async Task GetAllSections_Returns()
    {
        var sut = CreateServiceUnderTest();
        var secs = new List<QuestionnaireSectionEntry> { new() { Sys = new SystemDetails("S1") } };
        _repo.GetEntriesAsync<QuestionnaireSectionEntry>(Arg.Any<GetEntriesOptions>()).Returns(secs);

        var result = await sut.GetAllSectionsAsync();

        Assert.Equal(secs, result);
    }

    [Fact]
    public async Task GetAllSections_When_RepoThrows_Wraps()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<QuestionnaireSectionEntry>(Arg.Any<GetEntriesOptions>())
             .Returns<Task<IEnumerable<QuestionnaireSectionEntry>>>(_ => throw new Exception("boom"));

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() => sut.GetAllSectionsAsync());
    }

    // ---------- GetCategoryHeaderTextBySlugAsync ----------
    [Fact]
    public async Task GetCategoryHeaderTextBySlug_Returns_Text()
    {
        var sut = CreateServiceUnderTest();
        var cat = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = "Hello" }
        };
        _repo.GetEntriesAsync<QuestionnaireCategoryEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(new[] { cat });

        var text = await sut.GetCategoryHeaderTextBySlugAsync("networks");

        Assert.Equal("Hello", text);
    }

    [Fact]
    public async Task GetCategoryHeaderTextBySlug_Logs_And_Returns_Null_When_No_Text()
    {
        var sut = CreateServiceUnderTest();
        var cat = new QuestionnaireCategoryEntry { Header = new ComponentHeaderEntry() };
        _repo.GetEntriesAsync<QuestionnaireCategoryEntry>(Arg.Any<GetEntriesOptions>())
             .Returns([cat]);

        var text = await sut.GetCategoryHeaderTextBySlugAsync("missing");

        Assert.Null(text);
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }

    // ---------- GetCategoryBySlugAsync ----------
    [Fact]
    public async Task GetCategoryBySlug_Returns_First_And_Respects_IncludeLevel()
    {
        var sut = CreateServiceUnderTest();
        var cat = new QuestionnaireCategoryEntry { LandingPage = new() { Slug = "security" } };
        _repo.GetEntriesAsync<QuestionnaireCategoryEntry>(Arg.Is<GetEntriesOptions>(o => o.Include == 2))
             .Returns(new[] { cat });

        var res = await sut.GetCategoryBySlugAsync("security", includeLevel: 2);

        Assert.Same(cat, res);
    }

    [Fact]
    public async Task GetCategoryBySlug_Logs_And_Returns_Null_When_NotFound()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<QuestionnaireCategoryEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(Array.Empty<QuestionnaireCategoryEntry>());

        var res = await sut.GetCategoryBySlugAsync("nope");

        Assert.Null(res);
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }

    // ---------- GetIntroForMaturityAsync (case-insensitive) ----------
    [Fact]
    public async Task GetIntroForMaturity_Picks_By_Maturity_CaseInsensitive()
    {
        var sut = CreateServiceUnderTest();
        var intro = new RecommendationIntroEntry { Maturity = "Developing", Sys = new SystemDetails("I1") };
        var subRec = new SubtopicRecommendationEntry { Intros = new List<RecommendationIntroEntry> { intro } };
        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(new[] { subRec });

        var res = await sut.GetIntroForMaturityAsync("sub1", "developing");

        Assert.Same(intro, res);
    }

    [Fact]
    public async Task GetIntroForMaturity_Logs_And_Null_When_Subtopic_Missing()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(Array.Empty<SubtopicRecommendationEntry>());

        var res = await sut.GetIntroForMaturityAsync("sub1", "developing");

        Assert.Null(res);
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }

    [Fact]
    public async Task GetIntroForMaturity_Logs_And_Null_When_Maturity_NotFound()
    {
        var sut = CreateServiceUnderTest();
        var subRec = new SubtopicRecommendationEntry
        {
            Intros = new List<RecommendationIntroEntry>
        {
            new() { Maturity = "Secure" }
        }
        };
        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(new[] { subRec });

        var res = await sut.GetIntroForMaturityAsync("sub1", "developing");

        Assert.Null(res);
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }

    // ---------- GetPageBySlugAsync ----------
    [Fact]
    public async Task GetPageBySlug_Returns_First_And_Uses_Options_Include()
    {
        var sut = CreateServiceUnderTest();
        var page = new PageEntry { Sys = new SystemDetails("P1"), Slug = "about" };

        _repo.GetEntriesAsync<PageEntry>(Arg.Is<GetEntriesOptions>(o => o.Include == _pageOpts.Include))
             .Returns(new[] { page });

        var res = await sut.GetPageBySlugAsync("about");

        Assert.Same(page, res);
    }

    [Fact]
    public async Task GetPageBySlug_Throws_Wrapped_When_None()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<PageEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(Array.Empty<PageEntry>());

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() => sut.GetPageBySlugAsync("nope"));
    }

    [Fact]
    public async Task GetPageBySlug_Wraps_When_RepoThrows()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<PageEntry>(Arg.Any<GetEntriesOptions>())
             .Returns<Task<IEnumerable<PageEntry>>>(_ => throw new Exception("boom"));

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() => sut.GetPageBySlugAsync("about"));
    }

    // ---------- GetSectionBySlugAsync ----------
    [Fact]
    public async Task GetSectionBySlug_Returns_When_Found()
    {
        var sut = CreateServiceUnderTest();
        var sec = new QuestionnaireSectionEntry { Sys = new SystemDetails("S1") };

        _repo.GetEntriesAsync<QuestionnaireSectionEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(new[] { sec });

        var res = await sut.GetSectionBySlugAsync("intro");

        Assert.Same(sec, res);
    }

    [Fact]
    public async Task GetSectionBySlug_Throws_If_None()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<QuestionnaireSectionEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(Array.Empty<QuestionnaireSectionEntry>());

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() => sut.GetSectionBySlugAsync("missing"));
    }

    [Fact]
    public async Task GetSectionBySlug_Wraps_When_RepoThrows()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<QuestionnaireSectionEntry>(Arg.Any<GetEntriesOptions>())
             .Returns<Task<IEnumerable<QuestionnaireSectionEntry>>>(_ => throw new Exception("boom"));

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() => sut.GetSectionBySlugAsync("x"));
    }

    [Fact]
    public async Task GetSectionBySlug_Respects_IncludeLevel_When_Provided()
    {
        var sut = CreateServiceUnderTest();
        var sec = new QuestionnaireSectionEntry { Sys = new SystemDetails("S1") };

        _repo.GetEntriesAsync<QuestionnaireSectionEntry>(Arg.Is<GetEntriesOptions>(o => o.Include == 6))
             .Returns(new[] { sec });

        var res = await sut.GetSectionBySlugAsync("intro", includeLevel: 6);
        Assert.Same(sec, res);
    }

    // ---------- GetSubtopicRecommendationByIdAsync ----------
    [Fact]
    public async Task GetSubtopicRecommendationById_Returns_First_And_Respects_Include()
    {
        var sut = CreateServiceUnderTest();
        var sub = new SubtopicRecommendationEntry { Sys = new SystemDetails("S1") };

        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(Arg.Is<GetEntriesOptions>(o => o.Include == 4))
             .Returns(new[] { sub });

        var res = await sut.GetSubtopicRecommendationByIdAsync("sub1");

        Assert.Same(sub, res);
    }

    [Fact]
    public async Task GetSubtopicRecommendationById_Logs_And_Returns_Null_When_None()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(Array.Empty<SubtopicRecommendationEntry>());

        var res = await sut.GetSubtopicRecommendationByIdAsync("sub1");

        Assert.Null(res);
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }

    // ---------- GetSubtopicRecommendationIntroByIdAndMaturityAsync ----------
    [Fact]
    public async Task GetSubtopicRecommendationIntroByIdAndMaturity_Returns_When_Found_CaseSensitive()
    {
        // NOTE: your method uses case-sensitive Equals()
        var sut = CreateServiceUnderTest();
        var intro = new RecommendationIntroEntry { Sys = new SystemDetails("I1"), Maturity = "Developing" };
        var sub = new SubtopicRecommendationEntry { Intros = new List<RecommendationIntroEntry> { intro } };

        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(new[] { sub });

        var res = await sut.GetSubtopicRecommendationIntroByIdAndMaturityAsync("sub1", "Developing");
        Assert.Same(intro, res);
    }

    [Fact]
    public async Task GetSubtopicRecommendationIntroByIdAndMaturity_Logs_And_Null_When_Case_Differs()
    {
        // Because Equals() is case-sensitive here, "developing" will NOT match "Developing"
        var sut = CreateServiceUnderTest();
        var intro = new RecommendationIntroEntry { Sys = new SystemDetails("I1"), Maturity = "Developing" };
        var sub = new SubtopicRecommendationEntry { Intros = new List<RecommendationIntroEntry> { intro } };

        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(new[] { sub });

        var res = await sut.GetSubtopicRecommendationIntroByIdAndMaturityAsync("sub1", "developing");

        Assert.Null(res);
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }
}

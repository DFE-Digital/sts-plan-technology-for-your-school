using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class ContentfulServiceTests
{
    [Fact]
    public void Ctor_NullWorkflow_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ContentfulService(null!));
    }

    private static (
        ContentfulService contentfulService,
        IContentfulWorkflow contentfulWorkflow
    ) Build()
    {
        var contentfulWorkflow = Substitute.For<IContentfulWorkflow>();
        var contentfulService = new ContentfulService(contentfulWorkflow);
        return (contentfulService, contentfulWorkflow);
    }

    private static QuestionnaireCategoryEntry CreateCategory(string heading)
    {
        return new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = heading },
        };
    }

    [Fact]
    public async Task GetAllSections_Delegates_And_Returns()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        var expected = new List<QuestionnaireSectionEntry>
        {
            new() { Sys = new SystemDetails("S1") },
        };
        contentfulWorkflow.GetAllSectionsAsync().Returns(expected);

        var result = await contentfulService.GetAllSectionsAsync();

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetAllSectionsAsync();
    }

    [Fact]
    public async Task GetAllCategories_Delegates_And_Returns()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        var expected = new List<QuestionnaireCategoryEntry>
        {
            new() { Sys = new SystemDetails("S1") },
        };
        contentfulWorkflow.GetAllCategoriesAsync().Returns(expected);

        var result = await contentfulService.GetAllCategoriesAsync();

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetAllCategoriesAsync();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    public async Task GetCategoryBySlug_Forwards_IncludeLevel(int? include)
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string slug = "testSlug";
        var expected = new QuestionnaireCategoryEntry();

        contentfulWorkflow.GetCategoryBySlugAsync(slug, include).Returns(expected);

        var result = await contentfulService.GetCategoryBySlugAsync(slug, include);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetCategoryBySlugAsync(slug, include);
    }

    [Fact]
    public async Task GetLinkById_Uses_Generic_GetEntryById()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string id = "N1";
        var expected = new NavigationLinkEntry { Sys = new SystemDetails(id) };

        contentfulWorkflow.GetEntryByIdAsync<NavigationLinkEntry>(id).Returns(expected);

        var result = await contentfulService.GetLinkByIdAsync(id);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetEntryByIdAsync<NavigationLinkEntry>(id);
    }

    [Fact]
    public async Task GetNavigationLinks_Uses_Generic_GetEntries()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string id = "N1";
        var expected = new List<NavigationLinkEntry> { new() { Sys = new SystemDetails(id) } };

        contentfulWorkflow.GetEntriesAsync<NavigationLinkEntry>().Returns(expected);

        var result = await contentfulService.GetNavigationLinksAsync();

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetEntriesAsync<NavigationLinkEntry>();
    }

    [Fact]
    public async Task GetMicrocopyEntriesAsync_Uses_GetEntries()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string id = "M1";
        var expected = new List<MicrocopyEntry> { new() { Sys = new SystemDetails(id) } };

        contentfulWorkflow.GetEntriesAsync<MicrocopyEntry>().Returns(expected);

        var result = await contentfulService.GetMicrocopyEntriesAsync();

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetEntriesAsync<MicrocopyEntry>();
    }

    [Fact]
    public async Task GetPageById_Uses_Generic_GetEntryById()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string id = "P1";
        var expected = new PageEntry { Sys = new SystemDetails(id) };

        contentfulWorkflow.GetEntryByIdAsync<PageEntry>(id).Returns(expected);

        var result = await contentfulService.GetPageByIdAsync(id);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetEntryByIdAsync<PageEntry>(id);
    }

    [Fact]
    public async Task GetPageBySlug_Delegates_And_Returns()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string slug = "testSlug";
        var expected = new PageEntry { Slug = slug };

        contentfulWorkflow.GetPageBySlugAsync(slug).Returns(expected);

        var result = await contentfulService.GetPageBySlugAsync(slug);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetPageBySlugAsync(slug);
    }

    [Fact]
    public async Task GetQuestionById_Uses_Generic_GetEntryById()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string id = "Q1";
        var expected = new QuestionnaireQuestionEntry { Sys = new SystemDetails(id) };

        contentfulWorkflow.GetEntryByIdAsync<QuestionnaireQuestionEntry>(id).Returns(expected);

        var result = await contentfulService.GetQuestionByIdAsync(id);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetEntryByIdAsync<QuestionnaireQuestionEntry>(id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(3)]
    public async Task GetSectionBySlug_Forwards_IncludeLevel(int? include)
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string slug = "testSlug";
        var expected = new QuestionnaireSectionEntry();

        contentfulWorkflow.GetSectionBySlugAsync(slug, include).Returns(expected);

        var result = await contentfulService.GetSectionBySlugAsync(slug, include);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetSectionBySlugAsync(slug, include);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task GetPaginatedRecommendationEntries_Forwards_Page(int page)
    {
        var (contentfulService, contentfulWorkflow) = Build();
        var expected = new List<RecommendationChunkEntry>();

        contentfulWorkflow.GetPaginatedRecommendationEntriesAsync(page).Returns(expected);

        var result = await contentfulService.GetPaginatedRecommendationEntriesAsync(page);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetPaginatedRecommendationEntriesAsync(page);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 15)]
    public async Task GetRecommendationChunkCount_Forwards_Page(int page, int expected)
    {
        var (contentfulService, contentfulWorkflow) = Build();

        contentfulWorkflow.GetRecommendationChunkCountAsync().Returns(expected);

        var result = await contentfulService.GetRecommendationChunkCountAsync(page);

        Assert.Equal(expected, result);
        await contentfulWorkflow.Received(1).GetRecommendationChunkCountAsync();
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("2345")]
    public async Task GetTextBodyById_Uses_Generic_GetEntryById(string id)
    {
        var (contentfulService, contentfulWorkflow) = Build();
        var expected = new ComponentTextBodyEntry();

        contentfulWorkflow.GetEntryByIdAsync<ComponentTextBodyEntry>(id).Returns(expected);

        var result = await contentfulService.GetTextBodyByIdAsync(id);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetEntryByIdAsync<ComponentTextBodyEntry>(id);
    }
}

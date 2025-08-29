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

    private static (ContentfulService contentfulService, IContentfulWorkflow contentfulWorkflow) Build()
    {
        var contentfulWorkflow = Substitute.For<IContentfulWorkflow>();
        var contentfulService = new ContentfulService(contentfulWorkflow);
        return (contentfulService, contentfulWorkflow);
    }

    [Fact]
    public async Task GetAllSections_Delegates_And_Returns()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        var expected = new List<QuestionnaireSectionEntry> { new() { Sys = new SystemDetails("S1") } };
        contentfulWorkflow.GetAllSectionsAsync().Returns(expected);

        var result = await contentfulService.GetAllSectionsAsync();

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetAllSectionsAsync();
    }

    [Fact]
    public async Task GetCategoryHeaderTextBySlug_Delegates_And_Returns()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string slug = "testSlug";
        contentfulWorkflow.GetCategoryHeaderTextBySlugAsync(slug).Returns("Header");

        var result = await contentfulService.GetCategoryHeaderTextBySlugAsync(slug);

        Assert.Equal("Header", result);
        await contentfulWorkflow.Received(1).GetCategoryHeaderTextBySlugAsync(slug);
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

        contentfulWorkflow.GetEntryById<NavigationLinkEntry>(id).Returns(expected);

        var result = await contentfulService.GetLinkByIdAsync(id);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetEntryById<NavigationLinkEntry>(id);
    }

    [Fact]
    public async Task GetNavigationLinks_Uses_Generic_GetEntries()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string id = "N1";
        var expected = new List<NavigationLinkEntry> { new() { Sys = new SystemDetails(id) } };

        contentfulWorkflow.GetEntries<NavigationLinkEntry>().Returns(expected);

        var result = await contentfulService.GetNavigationLinksAsync();

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetEntries<NavigationLinkEntry>();
    }

    [Fact]
    public async Task GetPageById_Uses_Generic_GetEntryById()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string id = "P1";
        var expected = new PageEntry { Sys = new SystemDetails(id) };

        contentfulWorkflow.GetEntryById<PageEntry>(id).Returns(expected);

        var result = await contentfulService.GetPageByIdAsync(id);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetEntryById<PageEntry>(id);
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

        contentfulWorkflow.GetEntryById<QuestionnaireQuestionEntry>(id).Returns(expected);

        var result = await contentfulService.GetQuestionByIdAsync(id);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetEntryById<QuestionnaireQuestionEntry>(id);
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
    [InlineData(null)]
    [InlineData(1)]
    public async Task GetSubtopicRecommendationById_Forwards_IncludeLevel(int? include)
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string id = "S1";
        var expected = new SubtopicRecommendationEntry { Sys = new SystemDetails(id) };

        contentfulWorkflow.GetSubtopicRecommendationByIdAsync(id, include).Returns(expected);

        var result = await contentfulService.GetSubtopicRecommendationByIdAsync(id, include);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetSubtopicRecommendationByIdAsync(id, include);
    }

    [Fact]
    public async Task GetSubtopicRecommendationIntro_Delegates_And_Returns()
    {
        var (contentfulService, contentfulWorkflow) = Build();
        const string id = "S1";
        const string maturity = "medium";
        var expected = new RecommendationIntroEntry { Sys = new SystemDetails(id) };

        contentfulWorkflow.GetSubtopicRecommendationIntroByIdAndMaturityAsync(id, maturity).Returns(expected);

        var result = await contentfulService.GetSubtopicRecommendationIntroAsync(id, maturity);

        Assert.Same(expected, result);
        await contentfulWorkflow.Received(1).GetSubtopicRecommendationIntroByIdAndMaturityAsync(id, maturity);
    }
}

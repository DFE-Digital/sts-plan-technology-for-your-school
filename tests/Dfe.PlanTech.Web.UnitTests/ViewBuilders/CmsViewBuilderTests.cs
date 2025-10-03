using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.QaVisualiser;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class CmsViewBuilderTests
{
    // --- Helpers ------------------------------------------------------------

    private sealed class TestController : Controller { }

    private static CmsViewBuilder CreateSut(
        IContentfulService? contentful = null,
        IRecommendationService? recs = null)
    {
        contentful ??= Substitute.For<IContentfulService>();
        recs ??= Substitute.For<IRecommendationService>();
        return new CmsViewBuilder(contentful, recs);
    }

    private static QuestionnaireSectionEntry MakeSection(string id) =>
        new QuestionnaireSectionEntry { Sys = new SystemDetails(id), Name = $"Section {id}" };

    private static RecommendationChunkEntry MakeRecEntry(
        string header,
        params string?[] answerIds)
    {
        return new RecommendationChunkEntry
        {
            Header = header,
            Answers = answerIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => new QuestionnaireAnswerEntry { Sys = new SystemDetails(id!) })
                .ToList()
        };
    }

    // --- Ctor guards --------------------------------------------------------

    [Fact]
    public void Ctor_Null_Contentful_Throws()
    {
        var recs = Substitute.For<IRecommendationService>();
        Assert.Throws<System.ArgumentNullException>(() => new CmsViewBuilder(null!, recs));
    }

    [Fact]
    public void Ctor_Null_RecommendationService_Throws()
    {
        var contentful = Substitute.For<IContentfulService>();
        Assert.Throws<System.ArgumentNullException>(() => new CmsViewBuilder(contentful, null!));
    }

    // --- GetAllSectionsAsync ------------------------------------------------

    [Fact]
    public async Task GetAllSectionsAsync_Returns_SectionViewModels_For_All_Sections()
    {
        var contentful = Substitute.For<IContentfulService>();
        contentful.GetAllSectionsAsync().Returns(new[]
        {
            MakeSection("A"), MakeSection("B"), MakeSection("C")
        });

        var sut = CreateSut(contentful);

        var result = await sut.GetAllSectionsAsync();

        Assert.Equal(3, result.Count());
        Assert.All(result, vm => Assert.IsType<SectionViewModel>(vm));
        await contentful.Received(1).GetAllSectionsAsync();
    }

    // --- GetChunks ----------------------------------------------------------

    [Fact]
    public async Task GetChunks_Defaults_Page_To_1_When_Null()
    {
        var recs = Substitute.For<IRecommendationService>();
        recs.GetRecommendationChunkCount(1).Returns(5);
        recs.GetPaginatedRecommendationEntries(1).Returns(new[]
        {
            MakeRecEntry("H1", "a1", null),  // null id should be filtered out
            MakeRecEntry("H2", "b1", "b2")
        });

        var sut = CreateSut(recs: recs);
        var controller = new TestController();

        var action = await sut.GetChunks(controller, page: null);

        var ok = Assert.IsType<OkObjectResult>(action);
        var model = Assert.IsType<PagedResultViewModel<ChunkModel>>(ok.Value);

        Assert.Equal(1, model.Page);
        Assert.Equal(5, model.Total);

        // Expected chunks: "a1" from H1, and "b1","b2" from H2 => 3 total, no null IDs
        Assert.Equal(3, model.Items.Count);
        Assert.Contains(model.Items, i => i.AnswerId == "a1" && i.RecommendationHeader == "H1");
        Assert.Contains(model.Items, i => i.AnswerId == "b1" && i.RecommendationHeader == "H2");
        Assert.Contains(model.Items, i => i.AnswerId == "b2" && i.RecommendationHeader == "H2");

        await recs.Received(1).GetRecommendationChunkCount(1);
        await recs.Received(1).GetPaginatedRecommendationEntries(1);
    }

    [Fact]
    public async Task GetChunks_Uses_Supplied_Page_And_Flattens_Answers()
    {
        var recs = Substitute.For<IRecommendationService>();
        recs.GetRecommendationChunkCount(3).Returns(10);
        recs.GetPaginatedRecommendationEntries(3).Returns(new[]
        {
            MakeRecEntry("Header A", "x1"),
            MakeRecEntry("Header B", "y1", "y2", null), // null filtered
            MakeRecEntry("Header C") // no answers
        });

        var sut = CreateSut(recs: recs);
        var controller = new TestController();

        var action = await sut.GetChunks(controller, page: 3);

        var ok = Assert.IsType<OkObjectResult>(action);
        var model = Assert.IsType<PagedResultViewModel<ChunkModel>>(ok.Value);

        Assert.Equal(3, model.Page);
        Assert.Equal(10, model.Total);

        // Expected chunk models: x1 (A), y1 (B), y2 (B) => 3 total
        Assert.Equal(3, model.Items.Count);

        var (ids, headers) = (model.Items.Select(i => i.AnswerId).ToList(),
                              model.Items.Select(i => i.RecommendationHeader).ToList());

        Assert.Contains("x1", ids);
        Assert.Contains("y1", ids);
        Assert.Contains("y2", ids);

        Assert.Contains("Header A", headers);
        Assert.Contains("Header B", headers);
        Assert.DoesNotContain("Header C", headers); // no answers => contributes nothing

        await recs.Received(1).GetRecommendationChunkCount(3);
        await recs.Received(1).GetPaginatedRecommendationEntries(3);
    }

    [Fact]
    public async Task GetChunks_When_No_Entries_Returns_Empty_Items_With_Correct_Metadata()
    {
        var recs = Substitute.For<IRecommendationService>();
        recs.GetRecommendationChunkCount(2).Returns(0);
        recs.GetPaginatedRecommendationEntries(2).Returns(Enumerable.Empty<RecommendationChunkEntry>());

        var sut = CreateSut(recs: recs);
        var controller = new TestController();

        var action = await sut.GetChunks(controller, page: 2);

        var ok = Assert.IsType<OkObjectResult>(action);
        var model = Assert.IsType<PagedResultViewModel<ChunkModel>>(ok.Value);

        Assert.Equal(2, model.Page);
        Assert.Equal(0, model.Total);
        Assert.Empty(model.Items);
    }
}

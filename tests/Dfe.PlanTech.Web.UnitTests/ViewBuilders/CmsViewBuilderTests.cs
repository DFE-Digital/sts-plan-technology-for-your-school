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

    private static CmsViewBuilder CreateSut(IContentfulService? contentful = null)
    {
        contentful ??= Substitute.For<IContentfulService>();
        return new CmsViewBuilder(contentful);
    }

    private static QuestionnaireSectionEntry MakeSection(string id, List<QuestionnaireQuestionEntry>? questions = null) =>
        new QuestionnaireSectionEntry
        {
            Questions = questions ?? [],
            Sys = new SystemDetails(id),
            Name = $"Section {id}"
        };

    private static RecommendationChunkEntry MakeRecEntry(
       string header,
       params string?[] answerIds)
    {
        return new RecommendationChunkEntry
        {
            Header = header,
            CompletingAnswers = answerIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => new QuestionnaireAnswerEntry { Sys = new SystemDetails(id!) })
                .ToList(),
        };
    }

    // --- Ctor guards --------------------------------------------------------

    [Fact]
    public void Ctor_Null_Contentful_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CmsViewBuilder(null!));
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
        var contentful = Substitute.For<IContentfulService>();
        contentful.GetRecommendationChunkCountAsync(1).Returns(5);
        contentful.GetPaginatedRecommendationEntriesAsync(1).Returns(new[]
        {
            MakeRecEntry("H1", "a1", null),  // null id should be filtered out
            MakeRecEntry("H2", "b1"),
            MakeRecEntry("H3", "b2"),
        });

        var questions = new List<QuestionnaireQuestionEntry>
        {
            new QuestionnaireQuestionEntry()
            {
                Answers = new List<QuestionnaireAnswerEntry>
                {
                    new() { Sys = new SystemDetails("a1"), Text = "A1" },
                    new() { Sys = new SystemDetails("b1"), Text = "B1" },
                    new() { Sys = new SystemDetails("b2"), Text = "B2" },
                }
            }
        };

        contentful.GetAllSectionsAsync().Returns(new[]
        {
            MakeSection("A", questions),
        });


        var sut = CreateSut(contentful);
        var controller = new TestController();

        var action = await sut.GetChunks(controller, page: null);

        var ok = Assert.IsType<OkObjectResult>(action);
        var model = Assert.IsType<PagedResultViewModel<ChunkModel>>(ok.Value);

        Assert.Equal(1, model.Page);
        Assert.Equal(5, model.Total);

        // Expected chunks: "a1" from H1, and "b1" from H2 and "b3" from "h3" => 3 total, no null IDs
        Assert.Equal(3, model.Items.Count);
        Assert.Contains(model.Items, i => i.CompletingAnswerId == "a1" && i.RecommendationHeader == "H1");
        Assert.Contains(model.Items, i => i.CompletingAnswerId == "b1" && i.RecommendationHeader == "H2");
        Assert.Contains(model.Items, i => i.CompletingAnswerId == "b2" && i.RecommendationHeader == "H3");
    }

    [Fact]
    public async Task GetChunks_Uses_Supplied_Page_And_Flattens_Answers()
    {
        var contentful = Substitute.For<IContentfulService>();
        contentful.GetRecommendationChunkCountAsync(3).Returns(10);
        contentful.GetPaginatedRecommendationEntriesAsync(3).Returns(new[]
        {
            MakeRecEntry("Header A", "x1"),
            MakeRecEntry("Header B", "y1", null), // null filtered
            MakeRecEntry("Header C", "y2", null),
            MakeRecEntry("Header D") // no answers
        });

        var questions = new List<QuestionnaireQuestionEntry>
        {
            new QuestionnaireQuestionEntry()
            {
                Answers = new List<QuestionnaireAnswerEntry>
                {
                    new() { Sys = new SystemDetails("x1"), Text = "X1" },
                    new() { Sys = new SystemDetails("y1"), Text = "Y1" },
                    new() { Sys = new SystemDetails("y2"), Text = "Y2" },
                }
            }
        };

        contentful.GetAllSectionsAsync().Returns(new[]
        {
            MakeSection("A", questions),
        });

        var sut = CreateSut(contentful);
        var controller = new TestController();

        var action = await sut.GetChunks(controller, page: 3);

        var ok = Assert.IsType<OkObjectResult>(action);
        var model = Assert.IsType<PagedResultViewModel<ChunkModel>>(ok.Value);

        Assert.Equal(3, model.Page);
        Assert.Equal(10, model.Total);

        // Expected chunk models: x1 (A), y1 (B), y2 (C) => 3 total
        Assert.Equal(4, model.Items.Count);

        var (ids, headers) = (model.Items.Select(i => i.CompletingAnswerId).ToList(),
                              model.Items.Select(i => i.RecommendationHeader).ToList());

        Assert.Contains("x1", ids);
        Assert.Contains("y1", ids);
        Assert.Contains("y2", ids);

        Assert.Contains("Header A", headers);
        Assert.Contains("Header B", headers);
        Assert.Contains("Header C", headers);

        var headerD = model.Items.Single(i => i.RecommendationHeader == "Header D");
        Assert.Contains("", headerD.CompletingAnswerId); // completingAnswerId is empty string when no answers.
    }

    [Fact]
    public async Task GetChunks_When_No_Entries_Returns_Empty_Items_With_Correct_Metadata()
    {
        var sut = CreateSut();
        var controller = new TestController();

        var action = await sut.GetChunks(controller, page: 2);

        var ok = Assert.IsType<OkObjectResult>(action);
        var model = Assert.IsType<PagedResultViewModel<ChunkModel>>(ok.Value);

        Assert.Equal(2, model.Page);
        Assert.Equal(0, model.Total);
        Assert.Empty(model.Items);
    }
}

using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Options;
using Dfe.PlanTech.Data.Contentful.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class RecommendationWorkflowTests
{
    private readonly IContentfulRepository _repo = Substitute.For<IContentfulRepository>();

    private RecommendationWorkflow CreateServiceUnderTest() => new RecommendationWorkflow(_repo);

    [Fact]
    public void Ctor_NullRepo_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RecommendationWorkflow(null!));
    }

    [Fact]
    public async Task GetRecommendationChunkCount_Delegates_To_Repo_Ignores_Page_Param()
    {
        var serviceUnderTest = CreateServiceUnderTest();
        _repo.GetEntriesCount<RecommendationChunkEntry>().Returns(123);

        var count = await serviceUnderTest.GetRecommendationChunkCount(page: 7);

        Assert.Equal(123, count);
        await _repo.Received(1).GetEntriesCount<RecommendationChunkEntry>();
        // page parameter is intentionally not used by the workflow
    }

    [Fact]
    public async Task GetRecommendationChunkCount_Bubbles_Repo_Exception()
    {
        var serviceUnderTest = CreateServiceUnderTest();
        _repo.GetEntriesCount<RecommendationChunkEntry>()
             .Returns<Task<int>>(_ => throw new InvalidOperationException("boom"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => serviceUnderTest.GetRecommendationChunkCount(1));
    }

    [Fact]
    public async Task GetPaginatedRecommendationEntries_Passes_Include3_And_Page()
    {
        var serviceUnderTest = CreateServiceUnderTest();
        var page = 4;
        var expected = new List<RecommendationChunkEntry>
        {
            new RecommendationChunkEntry { Sys = new SystemDetails("rc-1") },
            new RecommendationChunkEntry { Sys = new SystemDetails("rc-2") },
        }.AsEnumerable();

        _repo.GetPaginatedEntriesAsync<RecommendationChunkEntry>(
                Arg.Is<GetEntriesOptions>(o => o.Include == 3 && o.Page == page))
            .Returns(expected);

        var result = await serviceUnderTest.GetPaginatedRecommendationEntries(page);

        Assert.Same(expected, result);
        await _repo.Received(1).GetPaginatedEntriesAsync<RecommendationChunkEntry>(
            Arg.Is<GetEntriesOptions>(o => o.Include == 3 && o.Page == page));
    }

    [Fact]
    public async Task GetPaginatedRecommendationEntries_Bubbles_Repo_Exception()
    {
        var serviceUnderTest = CreateServiceUnderTest();
        _repo.GetPaginatedEntriesAsync<RecommendationChunkEntry>(Arg.Any<GetEntriesOptions>())
             .Returns<Task<IEnumerable<RecommendationChunkEntry>>>(_ => throw new Exception("boom"));

        await Assert.ThrowsAsync<Exception>(() => serviceUnderTest.GetPaginatedRecommendationEntries(2));
    }
}

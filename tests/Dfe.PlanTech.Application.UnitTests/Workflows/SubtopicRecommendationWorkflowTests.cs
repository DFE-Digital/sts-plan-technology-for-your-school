using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Options;
using Dfe.PlanTech.Core.Contentful.Queries;
using Dfe.PlanTech.Data.Contentful.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class SubtopicRecommendationWorkflowTests
{
    private readonly ILogger<SubtopicRecommendationWorkflow> _logger
        = Substitute.For<ILogger<SubtopicRecommendationWorkflow>>();
    private readonly IContentfulRepository _repo = Substitute.For<IContentfulRepository>();

    private SubtopicRecommendationWorkflow CreateServiceUnderTest() => new(_logger, _repo);

    [Fact]
    public void Ctor_NullRepo_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SubtopicRecommendationWorkflow(_logger, null!));
    }

    // ---- GetFirstSubtopicRecommendationAsync ---------------------------------

    [Fact]
    public async Task GetFirstSubtopicRecommendation_Returns_First_And_Uses_Depth4_Query()
    {
        var sut = CreateServiceUnderTest();
        var subId = "sub-1";
        var entry = new SubtopicRecommendationEntry { Sys = new SystemDetails("rec-1") };

        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(
            Arg.Is<GetEntriesOptions>(o =>
                o.Include == 4 &&
                o.Queries!.OfType<ContentfulQuerySingleValue>().Any(q =>
                    q.Field == "fields.subtopic.sys.id" && (string)q.Value! == subId)))
        .Returns(new[] { entry });

        var result = await sut.GetFirstSubtopicRecommendationAsync(subId);

        Assert.Same(entry, result);
        await _repo.Received(1).GetEntriesAsync<SubtopicRecommendationEntry>(Arg.Any<GetEntriesOptions>());
    }

    [Fact]
    public async Task GetFirstSubtopicRecommendation_Logs_And_Returns_Null_When_Missing()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(Array.Empty<SubtopicRecommendationEntry>());

        var result = await sut.GetFirstSubtopicRecommendationAsync("sub-x");

        Assert.Null(result);
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }

    // ---- GetIntroForMaturityAsync --------------------------------------------

    [Fact]
    public async Task GetIntroForMaturity_Returns_Matching_Intro_And_Sets_Select_And_Depth2()
    {
        var sut = CreateServiceUnderTest();
        var subId = "sub-2";
        var intro = new RecommendationIntroEntry { Sys = new SystemDetails("i1"), Maturity = "Developing" };
        var rec = new SubtopicRecommendationEntry
        {
            Sys = new SystemDetails("rec-2"),
            Intros = new List<RecommendationIntroEntry> { intro }
        };

        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(
            Arg.Is<GetEntriesOptions>(o =>
                o.Include == 2 &&
                o.Select!.SequenceEqual(new[] { "fields.intros", "sys" }) &&
                o.Queries!.OfType<ContentfulQuerySingleValue>().Any(q =>
                    q.Field == "fields.subtopic.sys.id" && (string)q.Value! == subId)))
        .Returns(new[] { rec });

        var result = await sut.GetIntroForMaturityAsync(subId, "Developing");

        Assert.Same(intro, result);
    }

    [Fact]
    public async Task GetIntroForMaturity_Logs_And_Returns_Null_When_Recommendation_Missing()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(Array.Empty<SubtopicRecommendationEntry>());

        var result = await sut.GetIntroForMaturityAsync("sub-missing", "Developing");

        Assert.Null(result);
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }

    [Fact]
    public async Task GetIntroForMaturity_Logs_And_Returns_Null_When_Maturity_Not_Found()
    {
        var sut = CreateServiceUnderTest();
        var rec = new SubtopicRecommendationEntry
        {
            Sys = new SystemDetails("rec-3"),
            Intros = new List<RecommendationIntroEntry>
            {
                new() { Sys = new SystemDetails("i-secure"), Maturity = "Secure" }
            }
        };

        _repo.GetEntriesAsync<SubtopicRecommendationEntry>(Arg.Any<GetEntriesOptions>())
             .Returns(new[] { rec });

        var result = await sut.GetIntroForMaturityAsync("sub-3", "Developing");

        Assert.Null(result);
        _logger.ReceivedWithAnyArgs(1).LogError(default, default!, "Error");
    }
}

using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests.Builders;

public sealed class QuestionnaireSectionEntryBuilder
{
    private string _id = "section-id";
    private string _name = "Network security";
    private RecommendationChunkEntry[] _coreRecommendations =
    [
        new RecommendationChunkEntryBuilder().Build(),
        new RecommendationChunkEntryBuilder()
            .WithId("chunk-2")
            .WithSlug("second-chunk")
            .WithHeader("Second chunk")
            .WithTextBodyId("text-body-2")
            .Build(),
        new RecommendationChunkEntryBuilder()
            .WithId("chunk-3")
            .WithSlug("third-chunk")
            .WithHeader("Third chunk")
            .WithTextBodyId("text-body-3")
            .Build(),
    ];

    public QuestionnaireSectionEntryBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public QuestionnaireSectionEntryBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public QuestionnaireSectionEntryBuilder WithCoreRecommendations(
        params RecommendationChunkEntry[] coreRecommendations
    )
    {
        _coreRecommendations = coreRecommendations;
        return this;
    }

    public QuestionnaireSectionEntry Build() =>
        new()
        {
            Sys = new SystemDetails(_id),
            Name = _name,
            CoreRecommendations = [.. _coreRecommendations],
        };
}

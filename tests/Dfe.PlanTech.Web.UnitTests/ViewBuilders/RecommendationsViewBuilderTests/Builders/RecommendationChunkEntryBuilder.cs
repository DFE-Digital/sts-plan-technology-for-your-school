using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests.Builders;

public sealed class RecommendationChunkEntryBuilder
{
    private string _id = "chunk-1";
    private string _slug = "first-chunk";
    private string _header = "First chunk";
    private string _textBodyId = "text-body-1";
    private List<RelatedActionEntry> _relatedActions = [];

    public RecommendationChunkEntryBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public RecommendationChunkEntryBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    public RecommendationChunkEntryBuilder WithHeader(string header)
    {
        _header = header;
        return this;
    }

    public RecommendationChunkEntryBuilder WithTextBodyId(string textBodyId)
    {
        _textBodyId = textBodyId;
        return this;
    }

    public RecommendationChunkEntryBuilder WithRelatedActions(
        params RelatedActionEntry[] relatedActions
    )
    {
        _relatedActions = [.. relatedActions];
        return this;
    }

    public RecommendationChunkEntry Build() =>
        new()
        {
            Sys = new SystemDetails(_id),
            Slug = _slug,
            Header = _header,
            Content = [new ComponentTextBodyEntry { Sys = new SystemDetails(_textBodyId) }],
            RelatedActions = _relatedActions,
        };
}

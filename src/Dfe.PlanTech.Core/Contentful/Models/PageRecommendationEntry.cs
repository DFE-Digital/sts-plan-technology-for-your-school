namespace Dfe.PlanTech.Core.Contentful.Models;

public class PageRecommendationEntry : ContentfulEntry
{
    public string InternalName { get; init; } = null!;
    public ComponentTitleEntry? Title { get; init; }
    public ComponentInsetTextEntry? InsetText { get; init; }
    public ComponentTextBodyEntry? TextBody { get; init; }
    public IEnumerable<ComponentHeaderEntry>? Header { get; init; }
    public IEnumerable<ComponentTextBodyWithMaturityEntry>? TextBodyWithMaturity { get; init; }
    public List<ContentfulEntry> Content { get; init; } = [];
}

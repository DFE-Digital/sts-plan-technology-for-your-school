using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class PageRecommendationEntry: TransformableEntry<PageRecommendationEntry, CmsPageRecommendationDto>
{
    public string InternalName { get; init; } = null!;
    public ComponentTitleEntry? Title { get; init; }
    public ComponentInsetTextEntry? InsetText { get; init; }
    public ComponentTextBodyEntry? TextBody { get; init; }
    public IEnumerable<ComponentHeaderEntry>? Header { get; init; }
    public IEnumerable<ComponentTextBodyWithMaturityEntry>? TextBodyWithMaturity { get; init; }
    public List<ContentfulEntry> Content { get; init; } = [];

    protected override Func<PageRecommendationEntry, CmsPageRecommendationDto> Constructor => entry => new(entry);
}

using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class PageRecommendationEntry: TransformableEntry<PageRecommendationEntry, CmsPageRecommendationDto>, IContentfulEntry
{
    public string InternalName { get; init; } = null!;
    public ComponentTitleEntry? Title { get; init; }
    public ComponentInsetTextEntry? InsetText { get; init; }
    public ComponentTextBodyEntry? TextBody { get; init; }
    public IEnumerable<ComponentHeaderEntry>? Header { get; init; }
    public IEnumerable<ComponentTextBodyWithMaturityEntry>? TextBodyWithMaturity { get; init; }
    public List<ContentComponent> Content { get; init; } = [];

    public PageRecommendationEntry() : base(entry => new CmsPageRecommendationDto(entry)) {}
}

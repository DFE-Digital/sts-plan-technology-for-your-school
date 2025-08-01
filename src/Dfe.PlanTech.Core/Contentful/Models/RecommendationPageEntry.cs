using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RecommendationPageEntry: TransformableEntry<RecommendationPageEntry, CmsRecommendationPageDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Maturity { get; init; } = null!;
    public PageEntry Page { get; set; } = null!;

    public RecommendationPageEntry() : base(entry => new CmsRecommendationPageDto(entry)) {}
}

using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RecommendationPageEntry: TransformableEntry<RecommendationPageEntry, CmsRecommendationPageDto>
{
    public string InternalName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Maturity { get; init; } = null!;
    public PageEntry Page { get; set; } = null!;

    protected override Func<RecommendationPageEntry, CmsRecommendationPageDto> Constructor => entry => new(entry);
}

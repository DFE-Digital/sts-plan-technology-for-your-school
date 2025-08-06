using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RecommendationIntroEntry: TransformableEntry<RecommendationIntroEntry, CmsRecommendationIntroDto>
{
    public string InternalName { get; set; } = null!;
    public string Slug { get; init; } = null!;
    public ComponentHeaderEntry Header { get; init; } = null!;

    [NotMapped]
    public List<ContentfulEntry> Content { get; init; } = [];
    public string Maturity { get; init; } = null!;

    protected override Func<RecommendationIntroEntry, CmsRecommendationIntroDto> Constructor => entry => new(entry);
}

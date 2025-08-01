using System.ComponentModel.DataAnnotations.Schema;
using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RecommendationIntroEntry: TransformableEntry<RecommendationIntroEntry, CmsRecommendationIntroDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Slug { get; init; } = null!;
    public ComponentHeaderEntry Header { get; init; } = null!;

    [NotMapped]
    public List<Entry<ContentComponent>> Content { get; init; } = [];
    public string Maturity { get; init; } = null!;

    public RecommendationIntroEntry() : base(entry => new CmsRecommendationIntroDto(entry)) {}
}

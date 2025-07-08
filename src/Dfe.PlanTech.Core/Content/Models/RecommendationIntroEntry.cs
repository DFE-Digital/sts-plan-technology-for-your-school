using System.ComponentModel.DataAnnotations.Schema;
using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class RecommendationIntroEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public string Slug { get; init; } = null!;
    public ComponentHeaderEntry Header { get; init; } = null!;

    [NotMapped]
    public List<ContentComponent> Content { get; init; } = [];
    public string Maturity { get; init; } = null!;
}

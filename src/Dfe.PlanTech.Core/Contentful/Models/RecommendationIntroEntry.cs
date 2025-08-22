using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.Contentful.Interfaces;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RecommendationIntroEntry : ContentfulEntry, IHeaderWithContent
{
    [NotMapped]
    public List<ContentfulEntry> Content { get; init; } = [];
    public string Maturity { get; init; } = null!;
    public ComponentHeaderEntry Header { get; init; } = null!;
    public string HeaderText { get; } = null!;
    public string InternalName { get; set; } = null!;
    public string LinkText { get; } = null!;
    public string SlugifiedLinkText { get; } = null!;
    public string Slug { get; init; } = null!;
}

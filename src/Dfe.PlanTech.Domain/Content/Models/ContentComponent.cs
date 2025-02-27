using Contentful.Core.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;

namespace Dfe.PlanTech.Domain.Content.Models;

public abstract class ContentComponent : IContentComponent
{
    public SystemDetails Sys { get; init; } = null!;
    public new Fields Fields { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
    public string SummaryLine { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Meta { get; set; } = null!;
    public string ImageAlt { get; set; } = null!;
    public string Uri { get; set; } = null!;
    public Image Image { get; set; } = null!;
    public List<Target> Content { get; set; } = [];
    public new string Title { get; set; } = null!;
}

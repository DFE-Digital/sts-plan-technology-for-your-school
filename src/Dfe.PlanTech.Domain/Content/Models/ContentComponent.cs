using Contentful.Core.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;

namespace Dfe.PlanTech.Domain.Content.Models;

public abstract class ContentComponent : IContentComponent
{
    public SystemDetails Sys { get; init; } = null!;
    public new Fields Fields { get; init; } = null!;
    public Asset Asset { get; init; } = null!;
    public string SummaryLine { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string Meta { get; init; } = null!;
    public string ImageAlt { get; init; } = null!;
    public string Uri { get; init; } = null!;
    public Image Image { get; init; } = null!;
    public List<Target> Content { get; init; } = [];
    public new string Title { get; init; } = null!;
}

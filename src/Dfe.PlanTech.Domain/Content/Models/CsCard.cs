using Contentful.Core.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class CsCard : ContentComponent, IContentComponent
{
    public string? InternalName { get; set; }
    public string? Title { get; set; }
    public string? Meta { get; set; }
    public Asset? Image { get; set; }
    public string? ImageAlt { get; set; }
    public string? Uri { get; set; }
}

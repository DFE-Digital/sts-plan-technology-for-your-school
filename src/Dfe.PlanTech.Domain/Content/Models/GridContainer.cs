using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class GridContainer : ContentComponent, IContentComponent
{
    public string? InternalName { get; set; }
    public List<CsCard> Content { get; set; }

}

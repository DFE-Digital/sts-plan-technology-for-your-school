using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Core.Content.Models;

public class GridContainer : ContentComponent, IContentComponent
{
    public string? InternalName { get; set; }
    public IReadOnlyList<CsCard>? Content { get; set; }
}

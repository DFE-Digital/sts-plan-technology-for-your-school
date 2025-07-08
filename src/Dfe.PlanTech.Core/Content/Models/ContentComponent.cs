namespace Dfe.PlanTech.Core.Content.Models;

public abstract class ContentComponent
{
    public SystemDetails Sys { get; init; } = null!;
    public string Description { get; init; } = null!;
}

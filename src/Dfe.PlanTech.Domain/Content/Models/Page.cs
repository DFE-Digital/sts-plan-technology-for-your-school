namespace Dfe.PlanTech.Domain.Content.Models;

public class Page
{
    public string InternalName { get; init; } = null!;

    public string Slug { get; init; } = null!;

    public Title? Title { get; init; }
}

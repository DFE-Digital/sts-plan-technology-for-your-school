using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class Accordion : IAccordion
{
    public int Order { get; init; }
    public string Header { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string Title { get; init; } = null!;
}

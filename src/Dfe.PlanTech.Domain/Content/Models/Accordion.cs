using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class Accordion : IAccordion
{
    public int Order { get; set; }
    public string Header { get; set; }
    public string Link { get; set; }
}
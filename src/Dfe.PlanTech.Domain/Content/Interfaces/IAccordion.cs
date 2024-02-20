namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IAccordion
{
    public int Order { get; set; }
    public string Header { get; set; }
    public string Link { get; set; }
}
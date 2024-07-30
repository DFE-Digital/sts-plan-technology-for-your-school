namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IAccordion
{
    public int Order { get; }
    public string Header { get; }
    public string Slug { get; }
    public string Title { get; }
}

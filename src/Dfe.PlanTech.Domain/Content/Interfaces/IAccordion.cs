namespace Dfe.PlanTech.Domain.Content.Interfaces;

//TODO: just for testing purposes, will change back to interface after initial creation of view
public class IAccordion
{
    public string Header { get; set; }

    public string Link { get; set; }

    public bool IsExpanded { get; set; }
}
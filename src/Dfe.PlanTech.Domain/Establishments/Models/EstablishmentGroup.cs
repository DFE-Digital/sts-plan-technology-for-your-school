namespace Dfe.PlanTech.Domain.Establishments.Models;

public class EstablishmentGroup
{
    public int Id { get; set; }

    public string Uid { get; set; }

    public string GroupName { get; set; }

    public string GroupType { get; set; }

    public string GroupStatus { get; set; }

    public List<EstablishmentLink> EstablishmentLinks { get; set; } = new();
}

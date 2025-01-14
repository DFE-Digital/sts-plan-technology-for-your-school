namespace Dfe.PlanTech.Domain.Establishments.Models;

public class EstablishmentLink
{
    public int Id { get; set; }

    public string GroupUid { get; set; }

    public string EstablishmentName { get; set; }

    public string Urn { get; set; }

    public EstablishmentGroup? Group { get; set; }
}

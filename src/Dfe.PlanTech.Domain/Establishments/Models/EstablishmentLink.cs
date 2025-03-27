namespace Dfe.PlanTech.Domain.Establishments.Models;

public class EstablishmentLink
{
    public int Id { get; set; }

    public string GroupUid { get; set; } = null!;

    public string EstablishmentName { get; set; } = null!;

    public int Urn { get; set; }
}

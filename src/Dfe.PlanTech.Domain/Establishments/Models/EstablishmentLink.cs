using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Domain.Establishments.Models;

public class EstablishmentLink
{
    public int Id { get; set; }

    public string GroupUid { get; set; } = null!;

    public string EstablishmentName { get; set; } = null!;

    public string Urn { get; set; } = null!;

    [NotMapped]
    public int? CompletedSectionsCount { get; set; }
}

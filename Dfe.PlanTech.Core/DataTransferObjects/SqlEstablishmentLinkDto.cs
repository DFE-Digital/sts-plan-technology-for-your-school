using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Core.DataTransferObjects;

public class SqlEstablishmentLinkDto
{
    public int Id { get; set; }

    public string GroupUid { get; set; } = null!;

    public string EstablishmentName { get; set; } = null!;

    public string Urn { get; set; } = null!;

    public int? CompletedSectionsCount { get; set; }
}

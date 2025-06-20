namespace Dfe.PlanTech.Core.DataTransferObjects;

public class SqlEstablishmentDto
{
    public int Id { get; set; }

    public required string EstablishmentRef { get; set; }

    public string? EstablishmentType { get; set; }

    public required string OrgName { get; set; }

    public string? GroupUid { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }
}

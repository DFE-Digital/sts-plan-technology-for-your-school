namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlEstablishmentDto : ISqlDto
{
    public int Id { get; set; }
    public string EstablishmentRef { get; set; } = null!;
    public string? EstablishmentType { get; set; }
    public string OrgName { get; set; } = null!;
    public string? GroupUid { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? DateLastUpdated { get; set; }
}

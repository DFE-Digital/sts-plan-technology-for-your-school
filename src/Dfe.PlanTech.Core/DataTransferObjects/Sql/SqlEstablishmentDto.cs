namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlEstablishmentDto : SqlDto
{
    public int Id { get; set; }
    public string EstablishmentRef { get; set; }
    public string? EstablishmentType { get; set; }
    public string OrgName { get; set; }
    public string? GroupUid { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? DateLastUpdated { get; set; }
}

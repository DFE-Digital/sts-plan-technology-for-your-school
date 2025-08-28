namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlEstablishmentLinkDto : ISqlDto
{
    public int Id { get; set; }
    public int? CompletedSectionsCount { get; set; }
    public string EstablishmentName { get; set; } = null!;
    public string GroupUid { get; set; } = null!;
    public string Urn { get; set; } = null!;
}

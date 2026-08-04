namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlGiasEstablishmentGroupDto : ISqlDto
{
    public int GroupUid { get; init; }
    public string? GroupId { get; init; }
    public string GroupName { get; init; } = null!;
    public int GroupStatusCode { get; init; }
    public int GroupTypeCode { get; init; }
    public DateTime SyncedAt { get; set; }
    public string? Ukprn { get; set; } = null!;
}

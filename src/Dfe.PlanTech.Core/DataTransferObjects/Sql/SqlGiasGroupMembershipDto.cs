namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlGiasGroupMembershipDto : ISqlDto
{
    public int Id { get; init; }
    public int Urn { get; init; }
    public int GroupUid { get; init; }
    public DateTime SyncedAt { get; set; }
}

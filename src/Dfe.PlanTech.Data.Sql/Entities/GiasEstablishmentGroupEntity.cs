using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class GiasEstablishmentGroupEntity
{
    [Required]
    public int GroupUid { get; init; }

    public string? GroupId { get; init; }

    [Required]
    public string GroupName { get; init; } = null!;

    [Required]
    public string GroupStatusCode { get; init; } = null!;

    [Required]
    public int GroupTypeCode { get; init; }

    [Required]
    public DateTime SyncedAt { get; set; }

    public string? Ukprn { get; set; } = null!;

    public IEnumerable<GiasGroupMembershipEntity> GroupMemberships { get; set; } = [];

    public SqlGiasEstablishmentGroupDto AsDto()
    {
        return new SqlGiasEstablishmentGroupDto
        {
            GroupUid = GroupUid,
            GroupId = GroupId,
            GroupName = GroupName,
            GroupStatusCode = GroupStatusCode,
            GroupTypeCode = GroupTypeCode,
            SyncedAt = SyncedAt,
            Ukprn = Ukprn,
        };
    }
}

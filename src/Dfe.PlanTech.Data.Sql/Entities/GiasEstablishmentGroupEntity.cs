using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

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

    public ICollection<GiasGroupMembershipEntity> GroupMemberships { get; set; } = new List<GiasGroupMembershipEntity>();

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

    public static Expression<Func<GiasEstablishmentGroupEntity, GroupEstablishmentDTO>> AsBasicGroupEstablishmentDto =>
        x => new GroupEstablishmentDTO
        {
            GroupUID = x.GroupUid,
            GroupID = x.GroupId,
            Name = x.GroupName,
            BasicEstablishments = x.GroupMemberships
                .SelectMany(m => m.Establishments)
                    .Select(e => new EstablishmentBasicDto()
                    {
                            Urn = e.Urn.ToString(),
                            Name = e.EstablishmentName

                    })
                    .ToList()
        };
}

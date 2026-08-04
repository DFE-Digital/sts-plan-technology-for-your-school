using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class GiasGroupMembershipEntity
{
    [Required]
    public int Id { get; init; }

    [Required]
    public int Urn { get; init; }

    [Required]
    public int GroupUid { get; init; }

    [Required]
    public DateTime SyncedAt { get; set; }

    public SqlGiasGroupMembershipDto AsDto()
    {
        return new SqlGiasGroupMembershipDto
        {
            Id = Id,
            Urn = Urn,
            GroupUid = GroupUid,
            SyncedAt = SyncedAt,
        };
    }
}

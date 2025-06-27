using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class EstablishmentGroupEntity : SqlEntity<SqlEstablishmentGroupDto>
{
    public int Id { get; set; }

    public string Uid { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string GroupType { get; set; } = null!;

    public string GroupStatus { get; set; } = null!;

    protected override SqlEstablishmentGroupDto CreateDto()
    {
        return new SqlEstablishmentGroupDto
        {
            Id = Id,
            Uid = Uid,
            GroupName = GroupName,
            GroupType = GroupType,
            GroupStatus = GroupStatus
        };
    }
}

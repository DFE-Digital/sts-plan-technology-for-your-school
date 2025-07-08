using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class EstablishmentGroupEntity
{
    public int Id { get; set; }

    public string Uid { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string GroupType { get; set; } = null!;

    public string GroupStatus { get; set; } = null!;

    public SqlEstablishmentGroupDto AsDto()
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

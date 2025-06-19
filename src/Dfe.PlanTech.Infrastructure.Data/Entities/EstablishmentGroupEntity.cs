using Dfe.PlanTech.Domain.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Entities;

public class EstablishmentGroupEntity
{
    public int Id { get; set; }

    public string Uid { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string GroupType { get; set; } = null!;

    public string GroupStatus { get; set; } = null!;

    public EstablishmentGroupDto ToDto()
    {
        return new EstablishmentGroupDto
        {
            Id = Id,
            Uid = Uid,
            GroupName = GroupName,
            GroupType = GroupType,
            GroupStatus = GroupStatus
        };
    }
}

namespace Dfe.PlanTech.Core.DataTransferObjects;

public class SqlEstablishmentGroupDto
{
    public int Id { get; set; }

    public string Uid { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string GroupType { get; set; } = null!;

    public string GroupStatus { get; set; } = null!;
}

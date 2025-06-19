namespace Dfe.PlanTech.Domain.DataTransferObjects;

public class EstablishmentGroupDto
{
    public int Id { get; set; }

    public string Uid { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string GroupType { get; set; } = null!;

    public string GroupStatus { get; set; } = null!;
}

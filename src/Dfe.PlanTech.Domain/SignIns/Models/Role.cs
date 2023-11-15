namespace Dfe.PlanTech.Domain.SignIns.Models;

public class Role
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string NumericId { get; set; } = null!;

    public Status Status { get; set; } = null!;

}
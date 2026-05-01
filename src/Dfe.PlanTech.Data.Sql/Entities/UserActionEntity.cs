namespace Dfe.PlanTech.Data.Sql.Entities;

public class UserActionEntity
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public int? EstablishmentId { get; set; }
    public int? MatEstablishmentId { get; set; }
    public string RequestedUrl { get; set; } = null!;
    public DateTime DateCreated { get; set; }
}

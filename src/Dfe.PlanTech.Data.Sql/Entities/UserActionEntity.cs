namespace Dfe.PlanTech.Data.Sql.Entities;

public class UserActionEntity
{
    public Guid Id { get; set; }
    public Guid? SessionId { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.Now;
    public required int UserId { get; set; }
    public int? EstablishmentId { get; set; }
    public int? MatEstablishmentId { get; set; }
    public required string RequestedUrl { get; set; }
}

namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlUserActionDto
{
    public Guid Id { get; set; }

    public Guid? SessionId { get; set; }

    public int UserId { get; set; }

    public int? EstablishmentId { get; set; }

    public int? MatEstablishmentId { get; set; }

    public string RequestedUrl { get; set; } = null!;

    public DateTime DateCreated { get; set; }
}

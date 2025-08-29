namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlAnswerDto : ISqlDto
{
    public int Id { get; init; }
    public string? AnswerText { get; init; } = null!;
    public string ContentfulSysId { get; init; } = null!;
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;
}

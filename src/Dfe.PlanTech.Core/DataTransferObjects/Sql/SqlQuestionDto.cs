namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlQuestionDto : ISqlDto
{
    public int Id { get; init; }
    public string? QuestionText { get; init; } = null!;
    public string ContentfulSysId { get; init; } = null!;
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;
}

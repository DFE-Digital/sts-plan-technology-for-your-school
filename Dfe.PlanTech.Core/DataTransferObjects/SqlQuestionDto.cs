namespace Dfe.PlanTech.Core.DataTransferObjects;

public class SqlQuestionDto
{
    public int Id { get; init; }

    public string? QuestionText { get; init; } = null!;

    public string ContentfulRef { get; init; } = null!;

    public DateTime DateCreated { get; init; } = DateTime.UtcNow;
}

namespace Dfe.PlanTech.Domain.DataTransferObjects;

public class QuestionDto
{
    public int Id { get; init; }

    public string? QuestionText { get; init; } = null!;

    public string ContentfulRef { get; init; } = null!;

    public DateTime DateCreated { get; init; } = DateTime.UtcNow;
}

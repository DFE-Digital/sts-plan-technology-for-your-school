using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Domain.DataTransferObjects;

public class AnswerDto
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string? AnswerText { get; init; } = null!;

    [Required]
    public string ContentfulRef { get; init; } = null!;

    [Required]
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;
}

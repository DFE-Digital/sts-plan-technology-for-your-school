using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Core.DataTransferObjects;

public class SqlAnswerDto
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string? AnswerText { get; init; } = null!;

    [Required]
    public string ContentfulSysId { get; init; } = null!;

    [Required]
    public DateTime DateCreated { get; init; } = DateTime.UtcNow;
}

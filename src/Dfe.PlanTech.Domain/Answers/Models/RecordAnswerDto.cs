using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Domain.Answers.Models;

public class RecordAnswerDto
{
    [Required]
    public string? AnswerText { get; init; } = null!;

    [Required]
    public string ContentfulRef { get; init; } = null!;
}
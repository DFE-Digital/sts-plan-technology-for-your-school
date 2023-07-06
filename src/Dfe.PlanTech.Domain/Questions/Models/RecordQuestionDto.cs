using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Domain.Questions.Models;

public class RecordQuestionDto
{
    [Required]
    public string? QuestionText { get; init; } = null!;

    [Required]
    public string ContentfulRef { get; init; } = null!;
}
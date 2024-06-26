using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Domain.Submissions.Models;

public class ResponseAnswer
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string? AnswerText { get; init; } = null!;

    [Required]
    public string ContentfulRef { get; init; } = null!;

    [Required]
    public DateTime DateCreated { get; private set; } = DateTime.UtcNow;
}
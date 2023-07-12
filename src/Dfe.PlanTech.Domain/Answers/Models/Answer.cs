namespace Dfe.PlanTech.Domain.Answers.Models;

using System.ComponentModel.DataAnnotations;

public class Answer
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
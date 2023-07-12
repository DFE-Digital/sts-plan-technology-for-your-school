namespace Dfe.PlanTech.Domain.Questions.Models;

using System.ComponentModel.DataAnnotations;

public class Question
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string? QuestionText { get; init; } = null!;

    [Required]
    public string ContentfulRef { get; init; } = null!;

    [Required]
    public DateTime DateCreated { get; private set; } = DateTime.UtcNow;
}
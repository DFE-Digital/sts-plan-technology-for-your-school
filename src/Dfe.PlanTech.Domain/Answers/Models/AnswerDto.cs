namespace Dfe.PlanTech.Domain.Answers.Models;

using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Content.Models;

public class AnswerDto : ContentComponent
{
    [Required]
    public int Id { get; init; }

    [Required]
    public string AnswerText { get; init; } = null!;

    [Required]
    public string ContentfulRef { get; init; } = null!;

    [Required]
    public DateTime DateCreated { get; private set; } = DateTime.UtcNow;
}
using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Answer : ContentComponent
{
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Actual answer
    /// </summary>
    /// <value></value>
    [Required]
    public string Text { get; init; } = null!;

    public Question? NextQuestion { get; init; }

    [Required]
    public DateTime DateCreated { get; } = DateTime.UtcNow;
}

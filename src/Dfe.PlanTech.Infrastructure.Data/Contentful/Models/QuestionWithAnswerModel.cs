using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Models;

public record QuestionWithAnswerModel
{
    [Required]
    public string QuestionSysId { get; init; } = null!;

    [Required]
    public string QuestionText { get; init; } = "";

    [Required]
    public string AnswerSysId { get; init; } = null!;

    [Required]
    public string AnswerText { get; init; } = "";

    [Required]
    public DateTime? DateCreated { get; init; } = null!;

    public string? QuestionSlug { get; set; }
}

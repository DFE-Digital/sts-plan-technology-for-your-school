using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class QuestionWithAnswer
{
    [Required]
    public string QuestionRef { get; init; } = null!;

    [Required]
    public string QuestionText { get; init; } = null!;

    [Required]
    public string AnswerRef { get; init; } = null!;

    [Required]
    public string AnswerText { get; init; } = null!;

    [Required]
    public DateTime? DateCreated { get; init; } = null!;

    public int SubmissionId { get; init; }
}
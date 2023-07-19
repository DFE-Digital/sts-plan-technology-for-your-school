namespace Dfe.PlanTech.Domain.Questionnaire.Models;

using System.ComponentModel.DataAnnotations;

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
}
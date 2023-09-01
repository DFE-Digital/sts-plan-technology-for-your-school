using Dfe.PlanTech.Domain.Questionnaire.Models;
using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.Models;

public class QuestionViewModel
{
    [Required]
    public Question Question { get; init; } = null!;

    public string? AnswerRef { get; init; }

    public string? Params { get; init; } = null!;

    public int? SubmissionId { get; init; }

    public string? NoSelectedAnswerErrorMessage { get; init; }
}

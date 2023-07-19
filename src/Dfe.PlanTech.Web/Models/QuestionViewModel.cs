using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class QuestionViewModel : BaseViewModel
{
    [Required]
    public Question Question { get; init; } = null!;

    public string? AnswerRef { get; init; }

    public string? Params { get; init; } = null!;

    public int? SubmissionId { get; init; }
}

using Dfe.PlanTech.Domain.Questionnaire.Models;
using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.Models;

public class QuestionViewModel : BaseViewModel
{
    [Required]
    public Question Question { get; init; } = null!;

    public string? AnswerRef { get; init; }

    public string? Params { get; init; } = null!;

    public int? SubmissionId { get; init; }
}
